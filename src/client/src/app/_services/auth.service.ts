import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, finalize, map } from 'rxjs';
import { environment } from 'src/environments/environment.development';
import { Account } from '../_models/account';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

const authUrl = `${environment.apiUrl}/auth`;
const userUrl = `${environment.apiUrl}/user`;
const emailUrl = `${environment.apiUrl}/email`;
const adminUrl = `${environment.apiUrl}/admin`;

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private accountSubject: BehaviorSubject<Account | null>;
  public account: Observable<Account | null>;

  constructor(private router: Router, private http: HttpClient) {
    this.accountSubject = new BehaviorSubject<Account | null>(null);
    this.account = this.accountSubject.asObservable();
  }

  public get accountValue() {
    //console.log('test value:', this.accountSubject.value);
    return this.accountSubject.value;
  }

  // auth methods

  register(account: Account) {
    return this.http.post(`${authUrl}/register`, account);
  }

  login(email: string, password: string) {
    return this.http
      .post<any>(
        `${authUrl}/login`,
        { email, password },
        { withCredentials: true }
      )
      .pipe(
        map((account) => {
          this.accountSubject.next(account);
          localStorage.setItem('account', JSON.stringify(account));
          this.startRefreshTokenTimer();
          return account;
        })
      );
  }

  refreshToken() {
    return this.http
      .post<any>(`${authUrl}/refresh`, {}, { withCredentials: true })
      .pipe(
        map((account) => {
          this.accountSubject.next(account);
          this.startRefreshTokenTimer();
          return account;
        })
      );
  }

  logout() {
    this.http
      .post<any>(`${authUrl}/logout`, {}, { withCredentials: true })
      .subscribe();
    this.stopRefreshTokenTimer();
    localStorage.removeItem('account');
    this.accountSubject.next(null);
    this.router.navigate(['/account/login']);
  }

  // password reset methods

  forgotPassword(email: string) {
    return this.http.post(`${emailUrl}/forgot-password`, { email });
  }

  validateResetToken(token: string) {
    return this.http.post(`${emailUrl}/validate-reset-token`, { token });
  }

  resetPassword(token: string, password: string, confirmPassword: string) {
    return this.http.post(`${emailUrl}/reset-password`, {
      token,
      password,
      confirmPassword,
    });
  }

  // confirm email methods

  confirmEmail(email: string) {
    return this.http.post(`${emailUrl}/confirm-email`, { email });
  }

  verifyEmail(token: string) {
    return this.http.post(`${emailUrl}/verify-email`, { token });
  }

  // change email methods

  changeEmailRequest(email: string) {
    return this.http.post(`${emailUrl}/change-email-request`, { email });
  }

  changeEmail(
    token: string,
    currentEmail: string,
    newEmail: string,
    password: string,
    confirmPassword: string
  ) {
    return this.http.post(`${emailUrl}/change-email`, {
      token,
      currentEmail,
      newEmail,
      password,
      confirmPassword,
    });
  }

  // change password methods

  changePasswordRequest(email: string) {
    return this.http.post(`${emailUrl}/change-password-request`, { email });
  }

  changePassword(
    token: string,
    oldPassword: string,
    newPassword: string,
    confirmNewPassword: string
  ) {
    return this.http.post(`${emailUrl}/change-password`, {
      token,
      oldPassword,
      newPassword,
      confirmNewPassword,
    });
  }

  // user methods

  getCurrentUser() {
    return this.http.get<Account>(`${userUrl}/user`);
  }

  getAll() {
    return this.http.get<Account[]>(`${userUrl}/users`);
  }

  getById(id: string) {
    return this.http.get<Account>(`${userUrl}/user-by-id/?id=${id}`);
  }

  updateUserInfo(id: string, params: any) {
    return this.http.put(`${userUrl}/update-user-info/?id=${id}`, params).pipe(
      map((account: any) => {
        // update the current account if it was updated
        if (account.id === this.accountValue?.id) {
          // publish updated account to subscribers
          account = { ...this.accountValue, ...account };
          this.accountSubject.next(account);
        }
        return account;
      })
    );
  }

  // admin methods

  create(params: any) {
    return this.http.post(`${adminUrl}/create-user`, params);
  }

  update(id: string, params: any) {
    return this.http.put(`${adminUrl}/update-user/?id=${id}`, params).pipe(
      map((account: any) => {
        // update the current account if it was updated
        if (account.id === this.accountValue?.id) {
          // publish updated account to subscribers
          account = { ...this.accountValue, ...account };
          this.accountSubject.next(account);
        }
        return account;
      })
    );
  }

  delete(id: string) {
    return this.http.delete(`${adminUrl}/delete-user/?id=${id}`).pipe(
      finalize(() => {
        // auto logout if the logged in account was deleted
        if (id === this.accountValue?.id) this.logout();
      })
    );
  }

  // helper methods

  private refreshTokenTimeout?: any;

  private startRefreshTokenTimer() {
    // parse json object from base64 encoded jwt token
    const jwtBase64 = this.accountValue!.jwt!.split('.')[1];
    const jwtToken = JSON.parse(window.atob(jwtBase64));

    // set a timeout to refresh the token a minute before it expires
    const expires = new Date(jwtToken.exp * 1000);
    const timeout = expires.getTime() - Date.now() - 60 * 1000;
    this.refreshTokenTimeout = setTimeout(
      () => this.refreshToken().subscribe(),
      timeout
    );
  }

  private stopRefreshTokenTimer() {
    clearTimeout(this.refreshTokenTimeout);
  }
}

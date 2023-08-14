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

  // email methods

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

  confirmEmail(email: string) {
    return this.http.post(`${emailUrl}/confirm-email`, { email });
  }

  verifyEmail(token: string) {
    return this.http.post(`${emailUrl}/verify-email`, { token });
  }

  changeEmail(
    currentEmail: string,
    newEmail: string,
    password: string,
    confirmPassword: string
  ) {
    return this.http.post(`${emailUrl}/change-email`, {
      currentEmail,
      newEmail,
      password,
      confirmPassword,
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

  // admin methods

  create(params: any) {
    return this.http.post(adminUrl, params);
  }

  update(id: string, params: any) {
    return this.http.put(`${adminUrl}/update-user/${id}`, params).pipe(
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
    return this.http.delete(`${adminUrl}/update-user/${id}`).pipe(
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

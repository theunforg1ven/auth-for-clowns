import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbToastrService } from '@nebular/theme';
import { first } from 'rxjs';
import { PasswordMatch } from 'src/app/_helpers/password-match.validator';
import { Account } from 'src/app/_models/account';
import { AuthService } from 'src/app/_services/auth.service';

enum TokenStatus {
  Validating,
  Valid,
  Invalid,
}

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.scss'],
})
export class ChangePasswordComponent implements OnInit {
  TokenStatus = TokenStatus;
  tokenStatus = TokenStatus.Validating;
  token?: string;
  account!: Account | null;
  form!: FormGroup;
  submitting = false;
  submitted = false;
  deleting = false;

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService,
    private toastrService: NbToastrService
  ) {}

  ngOnInit() {
    this.form = this.formBuilder.group(
      {
        oldPassword: ['', [Validators.minLength(6)]],
        newPassword: ['', [Validators.minLength(6)]],
        confirmNewPassword: [''],
      },
      {
        validator: PasswordMatch('newPassword', 'confirmNewPassword'),
      }
    );

    this.token = this.route.snapshot.queryParams['token'];

    // remove token from url to prevent http referer leakage
    this.router.navigate([], { relativeTo: this.route, replaceUrl: true });
  }

  get f() {
    return this.form.controls;
  }

  onChangeUserPassword() {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    this.submitting = true;
    this.authService;

    // easy way to update /profile page
    let changePassword;
    changePassword = () =>
      this.authService.changePassword(
        this.token!,
        this.f['oldPassword'].value,
        this.f['newPassword'].value,
        this.f['confirmNewPassword'].value
      );

    changePassword()
      .pipe(first())
      .subscribe({
        next: () => {
          this.authService.logout();
        },
        error: (error: any) => {
          console.error(error);
          this.toastrService.danger('Error', `Changes wan't saved`);
          this.submitting = false;
        },
      });
  }
}

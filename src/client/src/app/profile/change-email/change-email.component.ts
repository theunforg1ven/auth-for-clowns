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
  selector: 'app-change-email',
  templateUrl: './change-email.component.html',
  styleUrls: ['./change-email.component.scss'],
})
export class ChangeEmailComponent implements OnInit {
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
        oldEmail: [
          this.account?.email,
          [Validators.required, Validators.email],
        ],
        newEmail: [this.account?.email, ''],
        password: ['', [Validators.minLength(6)]],
        confirmPassword: [''],
      },
      {
        validator: PasswordMatch('password', 'confirmPassword'),
      }
    );

    this.token = this.route.snapshot.queryParams['token'];

    // remove token from url to prevent http referer leakage
    this.router.navigate([], { relativeTo: this.route, replaceUrl: true });
  }

  get f() {
    return this.form.controls;
  }

  onChangeUserEmail() {
    this.submitted = true;

    // stop here if form is invalid
    if (this.form.invalid) {
      return;
    }

    this.submitting = true;
    this.authService;

    // easy way to update /profile page
    let changeEmail;
    changeEmail = () =>
      this.authService.changeEmail(
        this.token!,
        this.f['oldEmail'].value,
        this.f['newEmail'].value,
        this.f['password'].value,
        this.f['confirmPassword'].value
      );

    changeEmail()
      .pipe(first())
      .subscribe({
        next: () => {
          this.authService.logout();
        },
        error: (error) => {
          console.error(error);
          this.toastrService.danger('Error', `Changes wan't saved`);
          this.submitting = false;
        },
      });
  }
}

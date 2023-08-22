import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { AuthService } from '../_services/auth.service';
import { Account } from '../_models/account';

@Component({ templateUrl: 'update.component.html' })
export class UpdateComponent implements OnInit {
  //account = this.authService.accountValue!;
  account!: Account | null;
  form!: FormGroup;
  submitting = false;
  submitted = false;
  deleting = false;

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.authService
      .getCurrentUser()
      .pipe(first())
      .subscribe((x) => {
        this.account = x;
        this.form.patchValue(x);
      });
    this.form = this.formBuilder.group(
      {
        firstName: [this.account?.firstName, Validators.required],
        lastName: [this.account?.lastName, Validators.required],
        username: [this.account?.username, Validators.required],
        // email: [this.account.email, [Validators.required, Validators.email]],
        // confirmEmail: [this.account.email, ''],
        // password: ['', [Validators.minLength(6)]],
        // confirmPassword: [''],
      }
      // {
      //   validator: PasswordMatch('password', 'confirmPassword'),
      // }
    );
  }

  // convenience getter for easy access to form fields
  get f() {
    return this.form.controls;
  }

  onUpdateUser() {
    this.submitted = true;

    // stop here if form is invalid
    if (this.form.invalid) {
      return;
    }

    this.submitting = true;
    this.authService;

    // easy way to update /profile page
    let saveAccount;
    saveAccount = () =>
      this.authService.updateUserInfo(this.account?.id!, this.form.value);

    saveAccount()
      .pipe(first())
      .subscribe({
        next: () => {
          this.router.navigateByUrl('/profile');
        },
        error: (error) => {
          console.error(error);
          this.submitting = false;
        },
      });
  }
}

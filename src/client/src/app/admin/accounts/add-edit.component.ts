import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { AuthService } from 'src/app/_services/auth.service';
import { PasswordMatch } from 'src/app/_helpers/password-match.validator';

@Component({ templateUrl: 'add-edit.component.html' })
export class AddEditComponent implements OnInit {
  form!: FormGroup;
  id?: string;
  title!: string;
  loading = false;
  submitting = false;
  submitted = false;

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.id = this.route.snapshot.params['id'];

    this.form = this.formBuilder.group(
      {
        title: ['', Validators.required],
        firstName: ['', Validators.required],
        lastName: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
        role: ['', Validators.required],
        // password only required in add mode
        password: [
          '',
          [Validators.minLength(6), ...(!this.id ? [Validators.required] : [])],
        ],
        confirmPassword: [''],
      },
      {
        validator: PasswordMatch('password', 'confirmPassword'),
      }
    );

    this.title = 'Create Account';
    if (this.id) {
      // edit mode
      this.title = 'Edit Account';
      this.loading = true;
      this.authService
        .getById(this.id)
        .pipe(first())
        .subscribe((x) => {
          this.form.patchValue(x);
          this.loading = false;
        });
    }
  }

  // convenience getter for easy access to form fields
  get f() {
    return this.form.controls;
  }

  onSubmit() {
    this.submitted = true;

    // stop here if form is invalid
    if (this.form.invalid) {
      return;
    }

    this.submitting = true;

    // create or update account based on id param
    let saveAccount;
    let message: string;
    if (this.id) {
      saveAccount = () => this.authService.update(this.id!, this.form.value);
      message = 'Account updated';
    } else {
      saveAccount = () => this.authService.create(this.form.value);
      message = 'Account created';
    }

    saveAccount()
      .pipe(first())
      .subscribe({
        next: () => {
          console.log(message, { keepAfterRouteChange: true });
          this.router.navigateByUrl('/admin/accounts');
        },
        error: (error) => {
          console.error(error);
          this.submitting = false;
        },
      });
  }
}

import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { AuthService } from '../_services/auth.service';
import { PasswordMatch } from '../_helpers/password-match.validator';

@Component({ templateUrl: 'update.component.html' })
export class UpdateComponent implements OnInit {
  account = this.authService.accountValue!;
  form!: FormGroup;
  submitting = false;
  submitted = false;
  deleting = false;

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.form = this.formBuilder.group(
      {
        title: [this.account.username, Validators.required],
        firstName: [this.account.firstName, Validators.required],
        lastName: [this.account.lastName, Validators.required],
        username: [this.account.username, Validators.required],
        role: [this.account.role, Validators.required],
        email: [this.account.email, [Validators.required, Validators.email]],
        confirmEmail: [this.account.email, ''],
        password: ['', [Validators.minLength(6)]],
        confirmPassword: [''],
      },
      {
        validator: PasswordMatch('password', 'confirmPassword'),
      }
    );
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
    this.authService
      .update(this.account.id!, this.form.value)
      .pipe(first())
      .subscribe({
        next: () => {
          this.router.navigate(['../'], { relativeTo: this.route });
        },
        error: (error) => {
          this.submitting = false;
          console.log(error);
        },
      });
  }

  onDelete() {
    if (confirm('Are you sure?')) {
      this.deleting = true;
      this.authService
        .delete(this.account.id!)
        .pipe(first())
        .subscribe(() => {
          console.log('Account deleted successfully');
        });
    }
  }
}

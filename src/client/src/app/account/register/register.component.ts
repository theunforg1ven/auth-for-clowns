import { Component, OnInit } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { first } from 'rxjs';
import { PasswordMatch } from 'src/app/_helpers/password-match.validator';
import { AuthService } from 'src/app/_services/auth.service';
import FormValidator from 'src/app/_helpers/formValidator';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  formValidator!: FormValidator;
  submitting = false;
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.registerForm = this.fb.group(
      {
        firstName: ['', Validators.required],
        lastName: ['', Validators.required],
        username: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
        confirmEmail: ['', [Validators.required, this.matchValues('email')]],
        password: ['', [Validators.required, Validators.minLength(8)]],
        confirmPassword: ['', Validators.required],
      },
      {
        validator: PasswordMatch('password', 'confirmPassword'),
      }
    );
    this.registerForm.controls['password'].valueChanges.subscribe({
      next: () =>
        this.registerForm.controls['confirmPassword'].updateValueAndValidity(),
    });
  }

  get f() {
    return this.registerForm.controls;
  }

  onRegister() {
    this.submitted = true;

    if (this.registerForm.invalid) return;

    this.submitting = true;
    this.authService
      .register(this.registerForm.value)
      .pipe(first())
      .subscribe({
        next: () => {
          console.log(
            'Registration successful, please check your email for verification instructions'
          );
          this.router.navigate(['../login'], { relativeTo: this.route });
        },
        error: (error) => {
          console.error(error);
          this.submitting = false;
        },
      });
  }

  // helper for email and confirmEmail validation

  private matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value
        ? null
        : { notMatching: true };
    };
  }
}

import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidatorFn, Validators,
} from '@angular/forms';
import FormValidator from 'src/app/helpers/formValidator';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  formValidator!: FormValidator;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      username: ['', Validators.required],
      email: ['', Validators.required],
      confirmEmail: ['', [Validators.required , this.matchValues('email')]],
      password: [
        '',
        [Validators.required, Validators.minLength(8), Validators.maxLength(16)],
      ],
      confirmPassword: [
        '',
        [Validators.required,  this.matchValues('password')],
      ],
    });
    this.registerForm.controls['password'].valueChanges.subscribe({
      next: () =>
        this.registerForm.controls['confirmPassword'].updateValueAndValidity(),
    });
  }

  onRegister() {
    if (this.registerForm.valid) {
      console.log(this.registerForm.value);
    } else {
      console.error('Form is not valid');
      this.formValidator.validateFormFields(this.registerForm);
    }
  }

  private matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value ? null : {notMatching: true}
    }
  }
}

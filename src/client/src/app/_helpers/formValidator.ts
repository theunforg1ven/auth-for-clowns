import { FormGroup, FormControl } from '@angular/forms';

export default class FormValidator {
  validateFormFields(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach((prop) => {
      const control = formGroup.get(prop);
      if (control instanceof FormControl) {
        control.markAsDirty({ onlySelf: true });
      } else if (control instanceof FormGroup) {
        this.validateFormFields(control);
      }
    });
  }
}

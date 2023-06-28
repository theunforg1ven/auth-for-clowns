import { Component } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { Role } from '../_models/role';

@Component({ templateUrl: 'details.component.html' })
export class DetailsComponent {
  account = this.authService.accountValue;

  constructor(private authService: AuthService) {}

  test() {
    console.log('current user', this.account);
  }
}

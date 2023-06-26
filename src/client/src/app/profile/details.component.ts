import { Component } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Component({ templateUrl: 'details.component.html' })
export class DetailsComponent {
    account = this.authService.accountValue;

    constructor(private authService: AuthService) { }
}

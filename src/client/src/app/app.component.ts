import { Component } from '@angular/core';
import { Role } from './_models/role';
import { Account } from './_models/account';
import { AuthService } from './_services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  Role = Role;
  account?: Account | null;

  constructor(private authService: AuthService) {
    this.authService.account.subscribe((a) => (this.account = a));
  }

  logout() {
    this.authService.logout();
  }
}

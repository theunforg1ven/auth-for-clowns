import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { Role } from '../_models/role';
import { Account } from '../_models/account';
import { take } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

@Component({ templateUrl: 'details.component.html' })
export class DetailsComponent implements OnInit {
  account!: Account | null;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.authService
      .getCurrentUser()
      .pipe(take(1))
      .subscribe({
        next: (account) => (this.account = account),
      });
  }
}

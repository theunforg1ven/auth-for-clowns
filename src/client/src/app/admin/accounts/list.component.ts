import { AuthService } from 'src/app/_services/auth.service';
import { Component, OnInit } from '@angular/core';
import { first } from 'rxjs/operators';

@Component({ templateUrl: 'list.component.html' })
export class ListComponent implements OnInit {
  accounts?: any[];

  constructor(private authService: AuthService) {}

  ngOnInit() {
    this.authService
      .getAll()
      .pipe(first())
      .subscribe((accounts) => (this.accounts = accounts));
  }

  deleteAccount(id: string) {
    const account = this.accounts!.find((x) => x.id === id);
    account.isDeleting = true;
    this.authService
      .delete(id)
      .pipe(first())
      .subscribe(() => {
        this.accounts = this.accounts!.filter((x) => x.id !== id);
      });
  }
}

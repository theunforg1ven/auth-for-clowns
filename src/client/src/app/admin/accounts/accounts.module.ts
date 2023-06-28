import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { AccountsRoutingModule } from './accounts-routing.module';
import { ListComponent } from './list.component';
import { AddEditComponent } from './add-edit.component';
import { NbEvaIconsModule } from '@nebular/eva-icons';
import {
  NbLayoutModule,
  NbIconModule,
  NbInputModule,
  NbButtonModule,
  NbUserModule,
} from '@nebular/theme';

@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AccountsRoutingModule,
    NbLayoutModule,
    NbIconModule,
    NbEvaIconsModule,
    NbInputModule,
    NbButtonModule,
    NbUserModule,
  ],
  declarations: [ListComponent, AddEditComponent],
})
export class AccountsModule {}

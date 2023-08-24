import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { ProfileRoutingModule } from './profile-routing.module';
import { LayoutComponent } from './layout.component';
import { DetailsComponent } from './details.component';
import { UpdateComponent } from './update.component';
import {
  NbButtonModule,
  NbIconModule,
  NbInputModule,
  NbLayoutModule,
  NbThemeModule,
  NbToastrModule,
  NbUserModule,
} from '@nebular/theme';
import { NbEvaIconsModule } from '@nebular/eva-icons';
import { ChangeEmailComponent } from './change-email/change-email.component';
import { ChangePasswordComponent } from './change-password/change-password.component';

@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ProfileRoutingModule,
    NbLayoutModule,
    NbIconModule,
    NbEvaIconsModule,
    NbInputModule,
    NbButtonModule,
    NbUserModule,
    NbToastrModule.forRoot(),
  ],
  declarations: [LayoutComponent, DetailsComponent, UpdateComponent, ChangeEmailComponent, ChangePasswordComponent],
})
export class ProfileModule {}

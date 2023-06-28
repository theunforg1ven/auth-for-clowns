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
  NbUserModule,
} from '@nebular/theme';
import { NbEvaIconsModule } from '@nebular/eva-icons';

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
  ],
  declarations: [LayoutComponent, DetailsComponent, UpdateComponent],
})
export class ProfileModule {}

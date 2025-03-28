import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ReactiveFormsModule} from '@angular/forms';
import {MatDialogModule} from '@angular/material/dialog';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import {MatSelectModule} from '@angular/material/select';
import {ConfirmDialogComponent} from './components/confirm-dialog/confirm-dialog.component';
import {BaseDialogComponent} from './components/base-dialog/base-dialog.component';
import {ViewOnlyDialogComponent} from './components/view-only-dialog/view-only-dialog.component';
import {EntitySelectorDialogComponent} from './components/entity-display-dialog/entity-display-dialog.component';
import {MatTableModule} from "@angular/material/table";
import {NotificationDialogComponent} from './components/notification-dialog/notification-dialog.component';
import {MatIconModule} from '@angular/material/icon';
import {MatDatepickerModule} from '@angular/material/datepicker';
import {MatNativeDateModule} from '@angular/material/core';


@NgModule({
    declarations: [
        BaseDialogComponent,
        ConfirmDialogComponent,
        EntitySelectorDialogComponent,
        NotificationDialogComponent,
        ViewOnlyDialogComponent
    ],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatTableModule,
        MatIconModule,
        MatDatepickerModule,
        MatNativeDateModule
    ],
    exports: [
        BaseDialogComponent,
        ConfirmDialogComponent,
        ViewOnlyDialogComponent,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatIconModule,
        MatTableModule,
        MatDatepickerModule,
        MatNativeDateModule,
        CommonModule
    ]
})
export class SharedModule {
}

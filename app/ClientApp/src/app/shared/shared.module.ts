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
import {EntitySelectorDialogComponent} from './components/entity-display-dialog/entity-display-dialog.component';
import {MatTableModule} from "@angular/material/table";
import {NotificationDialogComponent} from './components/notification-dialog/notification-dialog.component';
import {MatIconModule} from '@angular/material/icon';


@NgModule({
    declarations: [
        BaseDialogComponent,
        ConfirmDialogComponent,
        EntitySelectorDialogComponent,
        NotificationDialogComponent
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
        MatIconModule
    ],
    exports: [
        BaseDialogComponent,
        ConfirmDialogComponent,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatIconModule,
    ]
})
export class SharedModule {
}

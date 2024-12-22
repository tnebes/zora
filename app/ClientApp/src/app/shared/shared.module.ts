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

@NgModule({
    declarations: [
        BaseDialogComponent,
        ConfirmDialogComponent,
        EntitySelectorDialogComponent
    ],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatTableModule
    ],
    exports: [
        BaseDialogComponent,
        ConfirmDialogComponent,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule
    ]
})
export class SharedModule {
}
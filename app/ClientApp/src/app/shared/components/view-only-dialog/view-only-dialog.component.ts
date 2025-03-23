import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { DialogField, ViewOnlyDialogData } from '../base-dialog/base-dialog.component';

@Component({
    selector: 'app-view-only-dialog',
    templateUrl: './view-only-dialog.component.html',
    styleUrls: ['./view-only-dialog.component.scss']
})
export class ViewOnlyDialogComponent<T> {
    private dateFieldNames = ['startDate', 'dueDate', 'createdAt', 'updatedAt'];
    
    constructor(
        public dialogRef: MatDialogRef<ViewOnlyDialogComponent<T>>,
        @Inject(MAT_DIALOG_DATA) public data: ViewOnlyDialogData<T>
    ) {}

    public fieldHasValue(fieldName: string): boolean {
        return this.data.entity && 
               (this.data.entity as any)[fieldName] !== undefined && 
               (this.data.entity as any)[fieldName] !== null;
    }

    public getFieldValue(fieldName: string): any {
        return (this.data.entity as any)[fieldName];
    }
    
    public isDateField(fieldName: string): boolean {
        const field = this.data.fields.find(f => f.name === fieldName);
        return field?.isDate === true || this.dateFieldNames.includes(fieldName);
    }
} 
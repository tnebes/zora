import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ViewOnlyDialogData } from '../base-dialog/base-dialog.component';
import { Constants } from '../../../core/constants';

@Component({
    selector: 'app-view-only-dialog',
    templateUrl: './view-only-dialog.component.html',
    styleUrls: ['./view-only-dialog.component.scss']
})
export class ViewOnlyDialogComponent<T> implements OnInit {
    private dateFieldNames = ['startDate', 'dueDate', 'createdAt', 'updatedAt'];
    
    constructor(
        public dialogRef: MatDialogRef<ViewOnlyDialogComponent<T>>,
        @Inject(MAT_DIALOG_DATA) public data: ViewOnlyDialogData<T>
    ) {
        // Set the dialog width using the provided width or the default from Constants
        this.dialogRef.updateSize(this.data.width || Constants.VIEW_ONLY_DIALOG_WIDTH);
    }

    ngOnInit(): void {
        // Initialize component
    }

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
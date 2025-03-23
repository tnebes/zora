import {Component, Inject} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';

export interface DialogField {
    name: string;
    type: 'text' | 'email' | 'password' | 'select' | 'multiselect' | 'file' | 'date';
    label: string;
    required?: boolean;
    options?: Array<{ value: any; display: string }>;
    validators?: Array<any>;
    accept?: string;
    isDate?: boolean;
}

export interface BaseDialogData<T> {
    title: string;
    entity?: T;
    fields: DialogField[];
    mode: 'create' | 'edit';
}

export interface ViewOnlyDialogData<T> {
    title: string;
    entity: T;
    fields: DialogField[];
}

@Component({
    selector: 'app-base-dialog',
    templateUrl: './base-dialog.component.html',
    styleUrls: ['./base-dialog.component.scss']
})
export class BaseDialogComponent<T> {
    public form: FormGroup;
    public isSubmitting: boolean = false;

    constructor(
        public dialogRef: MatDialogRef<BaseDialogComponent<T>>,
        @Inject(MAT_DIALOG_DATA) public data: BaseDialogData<T>,
        private formBuilder: FormBuilder
    ) {
        this.form = this.createForm();
    }

    private createForm(): FormGroup {
        const group: { [key: string]: any } = {};

        this.data.fields.forEach(field => {
            const validators = field.validators || [];
            if (field.required) {
                validators.push(Validators.required);
            }

            let initialValue = this.data.entity ?
                (this.data.entity as any)[field.name] :
                field.type === 'multiselect' ? [] :
                    field.type === 'file' ? null : '';

            // Handle date fields
            if ((field.type === 'date' || field.isDate) && initialValue) {
                initialValue = new Date(initialValue);
            }

            group[field.name] = [initialValue, validators];
        });

        return this.formBuilder.group(group);
    }

    public getErrorMessage(fieldName: string): string {
        const control = this.form.get(fieldName);
        if (!control || !control.errors) return '';

        if (control.errors['required']) return 'This field is required';
        if (control.errors['email']) return 'Invalid email format';

        return 'Invalid input';
    }

    public onSubmit(): void {
        if (this.form.valid) {
            const hasFileField = this.data.fields.some(field => field.type === 'file');

            if (hasFileField) {
                const formData = new FormData();
                Object.keys(this.form.controls).forEach(key => {
                    const control = this.form.get(key);
                    if (control?.value instanceof File) {
                        formData.append(key, control.value);
                    } else {
                        formData.append(key, control?.value);
                    }
                });
                this.dialogRef.close(formData);
            } else {
                const formValue = {...this.form.value};
                
                // Process date values before submitting
                this.data.fields.forEach(field => {
                    if ((field.type === 'date' || field.isDate) && formValue[field.name] instanceof Date) {
                        formValue[field.name] = formValue[field.name].toISOString();
                    }
                });
                
                this.dialogRef.close(formValue);
            }
        }
    }

    public onFileSelected(event: Event, fieldName: string): void {
        const fileInput = event.target as HTMLInputElement;
        const file = fileInput.files?.[0];
        if (file) {
            this.form.get(fieldName)?.setValue(file);
        }
    }

    public getFileName(fieldName: string): string {
        const file = this.form.get(fieldName)?.value;
        return file?.name || '';
    }
}

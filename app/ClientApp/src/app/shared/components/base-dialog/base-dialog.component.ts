import {Component, Inject, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';

export interface DialogField {
    name: string;
    type: 'text' | 'email' | 'password' | 'select' | 'multiselect' | 'file' | 'date';
    label: string;
    required?: boolean;
    options?: Array<{ [key: string]: any }>;
    validators?: Array<any>;
    accept?: string;
    isDate?: boolean;
    value?: any;
    pattern?: string;
    min?: number;
    max?: number;
    searchable?: boolean;
    searchService?: any;
    searchMethod?: string;
    displayField?: string;
    valueField?: string;
}

export interface BaseDialogData<T> {
    title: string;
    entity?: T;
    fields: DialogField[];
    mode: 'create' | 'edit';
    hiddenData?: any;
}

export interface ViewOnlyDialogData<T> {
    title: string;
    entity: T;
    fields: DialogField[];
    width?: string;
}

@Component({
    selector: 'app-base-dialog',
    templateUrl: './base-dialog.component.html',
    styleUrls: ['./base-dialog.component.scss']
})
export class BaseDialogComponent<T> implements OnInit {
    public form: FormGroup;
    public isSubmitting: boolean = false;

    constructor(
        public dialogRef: MatDialogRef<BaseDialogComponent<T>>,
        @Inject(MAT_DIALOG_DATA) public data: BaseDialogData<T>,
        private formBuilder: FormBuilder
    ) {
        this.form = this.createForm();
    }

    ngOnInit(): void {
        this.data.fields.forEach(field => {
            if (field.type === 'select' && field.searchable) {
                this.loadOptions(field);
            }
        });
    }

    private loadOptions(field: DialogField): void {
        if (field.searchService && field.searchMethod) {
            const service = field.searchService as any;
            const method = field.searchMethod as keyof typeof service;
            
            if (typeof service[method] === 'function') {
                service[method]('').subscribe({
                    next: (response: any) => {
                        if (response && response.items) {
                            field.options = response.items.map((item: any) => {
                                const option: any = {};
                                option.display = item[field.displayField || 'username'];
                                option.value = item[field.valueField || 'id'];
                                return option;
                            });
                        }
                    },
                    error: (error: any) => {
                        console.error('Error loading options:', error);
                    }
                });
            }
        }
    }

    private createForm(): FormGroup {
        const group: { [key: string]: any } = {};

        this.data.fields.forEach(field => {
            const validators = field.validators || [];
            if (field.required) {
                validators.push(Validators.required);
            }

            let initialValue;
            if (this.data.entity) {
                initialValue = (this.data.entity as any)[field.name];
            } else {
                initialValue = field.value !== undefined ? field.value : 
                               field.type === 'multiselect' ? [] :
                               field.type === 'file' ? null : '';
            }

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
            this.isSubmitting = true;
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
                
                if (this.data.hiddenData) {
                    Object.keys(this.data.hiddenData).forEach(key => {
                        formData.append(key, this.data.hiddenData[key]);
                    });
                }
                
                this.dialogRef.close(formData);
            } else {
                let formValue = {...this.form.value};
                
                this.data.fields.forEach(field => {
                    if ((field.type === 'date' || field.isDate) && formValue[field.name] instanceof Date) {
                        formValue[field.name] = formValue[field.name].toISOString();
                    }
                });
                
                if (this.data.hiddenData) {
                    formValue = {
                        ...formValue,
                        ...this.data.hiddenData
                    };
                }
                
                this.dialogRef.close(formValue);
            }
        }
    }

    public onFileSelected(event: Event, fieldName: string): void {
        const fileInput = event.target as HTMLInputElement;
        const file = fileInput.files?.[0];
        if (file) {
            this.form.get(fieldName)?.setValue(file);
            
            if (fieldName === 'asset') {
                const nameField = this.form.get('name');
                const currentName = nameField?.value;
                
                if (!currentName || currentName === '') {
                    const fileName = file.name;
                    const lastDotIndex = fileName.lastIndexOf('.');
                    const nameWithoutExtension = lastDotIndex !== -1 ? 
                        fileName.substring(0, lastDotIndex) : 
                        fileName;
                    
                    nameField?.setValue(nameWithoutExtension);
                    
                }
            }
        }
    }

    public getFileName(fieldName: string): string {
        const file = this.form.get(fieldName)?.value;
        return file?.name || '';
    }

    public onSearchableSelectFocus(field: DialogField): void {
        if (!field.options || field.options.length === 0) {
            this.loadOptions(field);
        }
    }

    public onSearchInput(field: DialogField, searchTerm: string): void {
        if (field.searchService && field.searchMethod) {
            const service = field.searchService as any;
            const method = field.searchMethod as keyof typeof service;
            
            if (typeof service[method] === 'function') {
                service[method](searchTerm).subscribe({
                    next: (response: any) => {
                        if (response && response.items) {
                            field.options = response.items.map((item: any) => {
                                const option: any = {};
                                option.display = item[field.displayField || 'username'];
                                option.value = item[field.valueField || 'id'];
                                return option;
                            });
                        }
                    },
                    error: (error: any) => {
                        console.error('Error searching options:', error);
                    }
                });
            }
        }
    }
}

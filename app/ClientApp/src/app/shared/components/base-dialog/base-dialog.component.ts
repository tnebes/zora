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
        // Preload options for searchable selects with default values
        this.data.fields.forEach(field => {
            if (field.type === 'select' && field.searchable && field.value !== undefined) {
                this.preloadSelectedOptions(field);
            }
        });
    }

    private preloadSelectedOptions(field: DialogField): void {
        if (field.searchService && field.searchMethod) {
            const service = field.searchService as any;
            const method = field.searchMethod as keyof typeof service;
            
            if (typeof service[method] === 'function') {
                // First load a list of options
                service[method]('').subscribe({
                    next: (response: any) => {
                        if (response && response.items) {
                            // Map the items to options
                            const options = response.items.map((item: any) => {
                                const option: any = {};
                                option.display = item[field.displayField || 'username'];
                                option.value = item[field.valueField || 'id'];
                                return option;
                            });
                            
                            field.options = options;

                            // If we have a specific default value, ensure it's in the list
                            if (field.value && !options.some((o: any) => o.value === field.value)) {
                                // If default value not in initial list, try to fetch it specifically
                                this.fetchSpecificOption(field, field.value);
                            }
                        }
                    },
                    error: (error: any) => {
                        console.error('Error loading initial options:', error);
                    }
                });
            }
        }
    }

    private fetchSpecificOption(field: DialogField, value: any): void {
        // This would be a method that could fetch a specific user by ID
        // For simplicity, we'll create a placeholder option until the real one loads
        if (!field.options) {
            field.options = [];
        }
        const tempOption: any = {};
        tempOption.display = `Loading user ${value}...`;
        tempOption.value = value;
        field.options.push(tempOption);
    }

    private createForm(): FormGroup {
        const group: { [key: string]: any } = {};

        this.data.fields.forEach(field => {
            const validators = field.validators || [];
            if (field.required) {
                validators.push(Validators.required);
            }

            // Set initial value based on the context (edit or create)
            let initialValue;
            if (this.data.entity) {
                // Editing existing entity
                initialValue = (this.data.entity as any)[field.name];
            } else {
                // Creating new entity - use default value if provided
                initialValue = field.value !== undefined ? field.value : 
                               field.type === 'multiselect' ? [] :
                               field.type === 'file' ? null : '';
            }

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
                
                // Add hidden data to formData if present
                if (this.data.hiddenData) {
                    Object.keys(this.data.hiddenData).forEach(key => {
                        formData.append(key, this.data.hiddenData[key]);
                    });
                }
                
                this.dialogRef.close(formData);
            } else {
                let formValue = {...this.form.value};
                
                // Process date values before submitting
                this.data.fields.forEach(field => {
                    if ((field.type === 'date' || field.isDate) && formValue[field.name] instanceof Date) {
                        formValue[field.name] = formValue[field.name].toISOString();
                    }
                });
                
                // Merge with hidden data if present
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
        }
    }

    public getFileName(fieldName: string): string {
        const file = this.form.get(fieldName)?.value;
        return file?.name || '';
    }

    public onSearchableSelectFocus(field: DialogField): void {
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

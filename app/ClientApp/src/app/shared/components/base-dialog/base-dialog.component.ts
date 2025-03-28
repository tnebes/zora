import {Component, Inject, OnDestroy, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import {Subscription} from 'rxjs';

export interface DialogField {
    name: string;
    type: 'text' | 'email' | 'password' | 'select' | 'multiselect' | 'file' | 'date';
    label: string;
    required?: boolean;
    options?: Array<{ value: any; display: string; [key: string]: any }>;
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
export class BaseDialogComponent<T> implements OnInit, OnDestroy {
    public form: FormGroup;
    public isSubmitting: boolean = false;
    private subscriptions: Subscription[] = [];

    constructor(
        public dialogRef: MatDialogRef<BaseDialogComponent<T>>,
        @Inject(MAT_DIALOG_DATA) public data: BaseDialogData<T>,
        private formBuilder: FormBuilder
    ) {
        this.form = this.createForm();
    }

    ngOnInit(): void {
        this.initializeSearchableFields();
    }

    ngOnDestroy(): void {
        this.subscriptions.forEach(subscription => subscription.unsubscribe());
    }

    private initializeSearchableFields(): void {
        this.data.fields.forEach(field => {
            if ((field.type === 'select' || field.type === 'multiselect') && field.searchable) {
                if (field.options && field.options.length > 0) {
                    return;
                }
                this.loadOptions(field, '');
            }
        });
    }

    private loadOptions(field: DialogField, searchTerm: string = ''): void {
        this.onSearchInput(field, searchTerm);
    }

    private createForm(): FormGroup {
        const group: { [key: string]: any } = {};

        this.data.fields.forEach(field => {
            const validators = field.validators || [];
            if (field.required) {
                validators.push(Validators.required);
            }
            
            if (field.pattern) {
                validators.push(Validators.pattern(field.pattern));
            }
            
            if (field.min !== undefined) {
                validators.push(Validators.min(field.min));
            }
            
            if (field.max !== undefined) {
                validators.push(Validators.max(field.max));
            }

            if (field.type === 'email') {
                validators.push(Validators.email);
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
        if (control.errors['minlength']) {
            const requiredLength = control.errors['minlength'].requiredLength;
            return `Minimum length is ${requiredLength} characters`;
        }
        if (control.errors['maxlength']) {
            const requiredLength = control.errors['maxlength'].requiredLength;
            return `Maximum length is ${requiredLength} characters`;
        }
        if (control.errors['min']) {
            return `Minimum value is ${control.errors['min'].min}`;
        }
        if (control.errors['max']) {
            return `Maximum value is ${control.errors['max'].max}`;
        }
        if (control.errors['pattern']) {
            return 'Invalid format';
        }

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
                    const value = control?.value;
                    
                    if (!control || value === null || value === undefined) {
                        return;
                    }

                    if (value instanceof File) {
                        formData.append(key, value);
                    } else if (Array.isArray(value)) {
                        value.forEach((item: any) => {
                            formData.append(key, item);
                        });
                    } else {
                        formData.append(key, value);
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
            
            // Auto-populate name field if empty
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
        // Load options if they haven't been loaded already
        if (!field.options || field.options.length === 0) {
            this.loadOptions(field, '');
        }
    }

    public onSearchInput(field: DialogField, searchTerm: string): void {
        if (!field.searchService || !field.searchMethod) {
            return;
        }

        const service = field.searchService;
        let methodName: string;
        let params: any;
        
        // Handle different method naming patterns
        if (searchTerm.length < 3) {
            // For short search terms or initial load, try to use a "get all" method
            if (field.searchMethod.startsWith('search')) {
                // Convert searchXXXByTerm to getXXXs
                const entityName = field.searchMethod.replace('search', '').replace('ByTerm', '');
                methodName = `get${entityName}s`;
                
                // Check if the method exists, otherwise fall back to the search method with empty term
                if (typeof service[methodName] !== 'function') {
                    methodName = field.searchMethod;
                    params = '';
                } else {
                    params = { page: 1, pageSize: 50 };
                }
            } else {
                // Use the standard pattern from before
                methodName = 'get' + field.searchMethod.replace('find', '').replace('ByTerm', '') + 's';
                params = { page: 1, pageSize: 50 };
                
                // If the calculated method doesn't exist, fall back to the original method
                if (typeof service[methodName] !== 'function') {
                    methodName = field.searchMethod;
                    params = '';
                }
            }
        } else {
            // For search terms with 3+ characters, use the search method
            methodName = field.searchMethod;
            params = searchTerm;
        }
        
        if (typeof service[methodName] === 'function') {
            const subscription = service[methodName](params).subscribe({
                next: (response: any) => {
                    if (response && response.items) {
                        field.options = response.items.map((item: any) => ({
                            display: item[field.displayField || 'username'],
                            value: item[field.valueField || 'id']
                        }));
                    }
                },
                error: (error: any) => {
                    console.error(`Error fetching options with ${methodName}:`, error);
                }
            });
            
            this.subscriptions.push(subscription);
        } else {
            console.error(`Method ${methodName} not found on service:`, service);
        }
    }
}

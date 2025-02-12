import { Component, forwardRef } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { MatFormFieldControl } from '@angular/material/form-field';
import { Subject } from 'rxjs';

@Component({
    selector: 'app-file-input',
    template: `
        <input type="file" (change)="handleFileInput($event)">
    `,
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => FileInputComponent),
        multi: true
    },
    { provide: MatFormFieldControl, useExisting: FileInputComponent }]
})
export class FileInputComponent extends MatFormFieldControl<File> implements ControlValueAccessor  {
    value: File | null = null;
    onChange: (value: File) => void = () => {};
    onTouched: () => void = () => {};
    stateChanges = new Subject<void>();
    focused = false;
    errorState = false;
    controlType = 'file-input';
    id = `file-input-${Math.random().toString(36).substr(2, 9)}`;
    placeholder = '';
    required = false;
    disabled = false;
    empty = true;
    shouldLabelFloat = false;

    writeValue(value: File): void {
        this.value = value;
        this.stateChanges.next();
    }

    registerOnChange(fn: (value: File) => void): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: () => void): void {
        this.onTouched = fn;
    }

    setDescribedByIds(ids: string[]): void {}
    onContainerClick(event: MouseEvent): void {}

    handleFileInput(event: Event): void {
        const inputElement: HTMLInputElement = event.target as HTMLInputElement;
        const files: FileList | null = inputElement.files;
        if (files && files.length > 0) {
            this.value = files[0];
            this.onChange(files[0]);
            this.onTouched();
            this.stateChanges.next();
        }
    }
}
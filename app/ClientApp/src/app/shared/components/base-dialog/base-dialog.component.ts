import {Component, Inject} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';

export interface DialogField {
  name: string;
  type: 'text' | 'email' | 'password' | 'select' | 'multiselect';
  label: string;
  required?: boolean;
  options?: Array<{ value: any; display: string }>;
  validators?: Array<any>;
}

export interface BaseDialogData<T> {
  title: string;
  entity?: T;
  fields: DialogField[];
  mode: 'create' | 'edit';
}

@Component({
  selector: 'app-base-dialog',
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <mat-dialog-content>
        <div class="form-fields">
          <ng-container *ngFor="let field of data.fields">
            <mat-form-field *ngIf="field.type !== 'multiselect'">
              <mat-label>{{ field.label }}</mat-label>

              <input *ngIf="field.type === 'text' || field.type === 'email' || field.type === 'password'"
                     matInput
                     [type]="field.type"
                     [formControlName]="field.name"
                     [required]="field.required ?? false">

              <mat-select *ngIf="field.type === 'select'"
                         [formControlName]="field.name"
                         [required]="field.required ?? false">
                <mat-option *ngFor="let option of field.options"
                           [value]="option.value">
                  {{ option.display }}
                </mat-option>
              </mat-select>

              <mat-error *ngIf="form.get(field.name)?.errors">
                {{ getErrorMessage(field.name) }}
              </mat-error>
            </mat-form-field>

            <mat-form-field *ngIf="field.type === 'multiselect'">
              <mat-label>{{ field.label }}</mat-label>
              <mat-select [formControlName]="field.name"
                         [required]="field.required ?? false"
                         multiple>
                <mat-option *ngFor="let option of field.options"
                           [value]="option.value">
                  {{ option.display }}
                </mat-option>
              </mat-select>
            </mat-form-field>
          </ng-container>
        </div>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button type="button" mat-dialog-close>Cancel</button>
        <button mat-raised-button
                color="primary"
                type="submit"
                [disabled]="!form.valid || isSubmitting">
          {{ data.mode === 'create' ? 'Create' : 'Save' }}
        </button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    .form-fields {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    mat-form-field {
      width: 100%;
    }
  `]
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

      const initialValue = this.data.entity ?
        (this.data.entity as any)[field.name] :
        field.type === 'multiselect' ? [] : '';

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
      this.dialogRef.close(this.form.value);
    }
  }
}

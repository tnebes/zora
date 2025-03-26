import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDividerModule } from '@angular/material/divider';
import { AssetService } from '../../../core/services/asset.service';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AssetResponse } from '../../../core/models/asset.interface';
import { CommonModule, DatePipe } from '@angular/common';
import { UserService } from '../../../core/services/user.service';
import { UserResponse } from '../../../core/models/user.interface';
import { forkJoin, of } from 'rxjs';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { MatNativeDateModule } from '@angular/material/core';

export interface AssetDialogData {
  mode: 'view' | 'upload' | 'edit' | 'create';
  asset?: AssetResponse;
  taskId?: number;
}

const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
const ALLOWED_FILE_TYPES = [
  'image/jpeg',
  'image/png',
  'image/gif',
  'application/pdf',
  'application/msword',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  'application/vnd.ms-excel',
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  'text/plain'
];

@Component({
  selector: 'app-asset-unified',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatSnackBarModule,
    ReactiveFormsModule,
    MatTabsModule,
    MatDividerModule,
    MatNativeDateModule,
    DatePipe
  ],
  templateUrl: './asset-unified.component.html',
  styleUrls: ['./asset-unified.component.scss']
})
export class AssetUnifiedComponent implements OnInit {
  // View mode properties
  public isImage: boolean = false;
  public previewUrl: SafeUrl | null = null;
  public createdByUser: UserResponse | null = null;
  public updatedByUser: UserResponse | null = null;
  public isLoadingPreview: boolean = false;

  // Upload/Edit mode properties
  public uploadForm: FormGroup;
  public selectedFile: File | null = null;
  public uploading: boolean = false;
  public fileError: string | null = null;
  public isEditing: boolean = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: AssetDialogData,
    private readonly dialogRef: MatDialogRef<AssetUnifiedComponent>,
    private readonly formBuilder: FormBuilder,
    private readonly assetService: AssetService,
    private readonly snackBar: MatSnackBar,
    private readonly userService: UserService,
    private readonly sanitizer: DomSanitizer
  ) {
    this.uploadForm = this.formBuilder.group({
      name: ['', Validators.required],
      description: ['']
    });

    if (this.isViewMode && this.data.asset) {
      this.isImage = this.checkIfImage(this.data.asset.assetPath);
    }

    if (this.isEditMode && this.data.asset) {
      this.isEditing = true;
      this.uploadForm.patchValue({
        name: this.data.asset.name,
        description: this.data.asset.description
      });
    }
  }

  ngOnInit(): void {
    if (this.isViewMode && this.data.asset) {
      this.loadUsers();
      if (this.isImage) {
        this.loadPreview();
      }
    }
  }

  get isViewMode(): boolean {
    return this.data.mode === 'view';
  }

  get isUploadMode(): boolean {
    return this.data.mode === 'upload';
  }

  get isEditMode(): boolean {
    return this.data.mode === 'edit';
  }

  // View mode methods
  private loadUsers(): void {
    if (!this.data.asset) return;

    const createdByUser$ = this.userService.getUserById(this.data.asset.createdBy);
    const updatedByUser$ = this.data.asset.updatedBy 
      ? this.userService.getUserById(this.data.asset.updatedBy)
      : of(null);

    forkJoin({
      createdBy: createdByUser$,
      updatedBy: updatedByUser$
    }).subscribe(result => {
      this.createdByUser = result.createdBy;
      this.updatedByUser = result.updatedBy;
    });
  }

  private loadPreview(): void {
    if (!this.data.asset) return;

    this.isLoadingPreview = true;
    this.assetService.download(this.data.asset.id).subscribe({
      next: (blob) => {
        const objectUrl = URL.createObjectURL(blob);
        this.previewUrl = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
        this.isLoadingPreview = false;
      },
      error: () => {
        this.isLoadingPreview = false;
      }
    });
  }

  private checkIfImage(path: string): boolean {
    const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp'];
    return imageExtensions.some(ext => path.toLowerCase().endsWith(ext));
  }

  public downloadAsset(): void {
    if (!this.data.asset) return;

    this.assetService.download(this.data.asset.id).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      const originalExtension = this.data.asset?.assetPath.split('.').pop() || '';
      const fileName = this.data.asset?.name.includes('.') ? this.data.asset?.name : `${this.data.asset?.name}.${originalExtension}`;
      link.download = fileName;
      link.click();
      window.URL.revokeObjectURL(url);
    });
  }

  // Upload mode methods
  public onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      const file = input.files[0];
      
      // Set the asset name to the file name if the name field is empty
      const currentName = this.uploadForm.get('name')?.value;
      
      if (!currentName || currentName === '') {
        const fileName = file.name;
        const lastDotIndex = fileName.lastIndexOf('.');
        const nameWithoutExtension = lastDotIndex !== -1 ? 
          fileName.substring(0, lastDotIndex) : 
          fileName;
        
        this.uploadForm.get('name')?.setValue(nameWithoutExtension);
      }
      
      this.validateFile(file);
    }
  }

  private validateFile(file: File): void {
    this.fileError = null;

    if (file.size > MAX_FILE_SIZE) {
      this.fileError = 'File size exceeds 10MB limit';
      this.selectedFile = null;
      return;
    }

    if (!ALLOWED_FILE_TYPES.includes(file.type)) {
      this.fileError = 'File type not allowed';
      this.selectedFile = null;
      return;
    }

    this.selectedFile = file;
  }

  public onSubmit(): void {
    if (!this.uploadForm.valid || (this.isUploadMode && !this.selectedFile) || this.fileError) {
      return;
    }

    if (this.isEditMode) {
      this.updateAsset();
    } else {
      this.createAsset();
    }
  }

  private createAsset(): void {
    if (!this.selectedFile) return;
    
    this.uploading = true;
    const formData = new FormData();
    formData.append('name', this.uploadForm.get('name')?.value ?? '');
    formData.append('description', this.uploadForm.get('description')?.value ?? '');
    formData.append('asset', this.selectedFile);
    
    if (this.data.taskId) {
      formData.append('workAssetId', this.data.taskId.toString());
      formData.append('WorkAssetId', this.data.taskId.toString());
    }

    this.assetService.create(formData).subscribe({
      next: (response: AssetResponse) => {
        this.dialogRef.close(response);
        this.snackBar.open('Asset uploaded successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error creating asset:', error);
        
        let errorMessage = 'Failed to upload asset';
        if (error.error?.title) {
          errorMessage += `: ${error.error.title}`;
        } else if (error.error?.message) {
          errorMessage += `: ${error.error.message}`;
        } else if (error.message) {
          errorMessage += `: ${error.message}`;
        }
        
        this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
        this.uploading = false;
      },
      complete: () => {
        this.uploading = false;
      }
    });
  }

  private updateAsset(): void {
    if (!this.data.asset) return;
    
    this.uploading = true;
    const updateData: any = {
      id: this.data.asset.id,
      name: this.uploadForm.get('name')?.value ?? '',
      description: this.uploadForm.get('description')?.value ?? '',
      assetPath: this.data.asset.assetPath
    };

    this.assetService.update(updateData).subscribe({
      next: (response: AssetResponse) => {
        this.dialogRef.close(response);
        this.snackBar.open('Asset updated successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error updating asset:', error);
        this.snackBar.open('Failed to update asset', 'Close', { duration: 5000 });
        this.uploading = false;
      },
      complete: () => {
        this.uploading = false;
      }
    });
  }
} 
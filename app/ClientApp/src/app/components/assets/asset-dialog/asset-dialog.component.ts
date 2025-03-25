import {Component, Inject, OnInit} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogModule} from '@angular/material/dialog';
import {AssetResponse} from '../../../core/models/asset.interface';
import {MatTabsModule} from '@angular/material/tabs';
import {DatePipe, NgIf} from '@angular/common';
import {AssetService} from '../../../core/services/asset.service';
import {MatButtonModule} from '@angular/material/button';
import {MatDividerModule} from '@angular/material/divider';
import {UserService} from '../../../core/services/user.service';
import {UserResponse} from '../../../core/models/user.interface';
import {forkJoin, map, Observable, of} from 'rxjs';

@Component({
    selector: 'app-asset-dialog',
    template: `
    <h2 mat-dialog-title>{{data.name}}</h2>
    <mat-tab-group>
      <mat-tab label="Preview">
        <mat-dialog-content class="preview-content">
          <img *ngIf="isImage" [src]="data.assetPath" [alt]="data.name" class="image-preview">
          <div *ngIf="!isImage" class="download-section">
            <p>Preview not available for this file type</p>
            <button mat-raised-button color="primary" (click)="downloadAsset()">
              Download {{data.name}}
            </button>
          </div>
        </mat-dialog-content>
      </mat-tab>
      <mat-tab label="Metadata">
        <mat-dialog-content class="metadata-content">
          <div class="metadata-grid">
            <div class="metadata-item">
              <span class="metadata-label">ID</span>
              <span class="metadata-value">{{data.id}}</span>
            </div>
            <mat-divider></mat-divider>
            
            <div class="metadata-item">
              <span class="metadata-label">Name</span>
              <span class="metadata-value">{{data.name}}</span>
            </div>
            <mat-divider></mat-divider>
            
            <div class="metadata-item">
              <span class="metadata-label">Description</span>
              <span class="metadata-value">{{data.description || 'No description'}}</span>
            </div>
            <mat-divider></mat-divider>
            
            <div class="metadata-item">
              <span class="metadata-label">Asset Path</span>
              <span class="metadata-value">{{data.assetPath}}</span>
            </div>
            <mat-divider></mat-divider>
            
            <div class="metadata-item">
              <span class="metadata-label">Created By</span>
              <span class="metadata-value">{{createdByUser?.username || 'Loading...'}}</span>
            </div>
            <mat-divider></mat-divider>
            
            <div class="metadata-item">
              <span class="metadata-label">Created At</span>
              <span class="metadata-value">{{data.createdAt | date:'medium'}}</span>
            </div>
            <mat-divider></mat-divider>
            
            <div class="metadata-item">
              <span class="metadata-label">Updated By</span>
              <span class="metadata-value">{{updatedByUser?.username || 'Not updated'}}</span>
            </div>
            <mat-divider></mat-divider>
            
            <div class="metadata-item">
              <span class="metadata-label">Updated At</span>
              <span class="metadata-value">{{data.updatedAt ? (data.updatedAt | date:'medium') : 'Not updated'}}</span>
            </div>
          </div>
        </mat-dialog-content>
      </mat-tab>
    </mat-tab-group>
    <mat-dialog-actions>
      <button mat-raised-button mat-dialog-close class="close-button" color="primary">Close</button>
    </mat-dialog-actions>
  `,
    styles: [`
      .preview-content {
        padding: 20px;
        text-align: center;
      }
      .image-preview {
        max-width: 100%;
        max-height: 500px;
        object-fit: contain;
      }
      .download-section {
        padding: 20px;
        text-align: center;
      }
      .metadata-content {
        padding: 24px;
      }
      .metadata-grid {
        display: flex;
        flex-direction: column;
        gap: 16px;
      }
      .metadata-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 8px 0;
      }
      .metadata-label {
        font-weight: 500;
        color: rgba(0, 0, 0, 0.87);
        min-width: 120px;
      }
      .metadata-value {
        color: rgba(0, 0, 0, 0.67);
        text-align: right;
        word-break: break-all;
      }
      mat-divider {
        margin: 0;
      }
    `],
    standalone: true,
    imports: [MatTabsModule, MatDialogModule, DatePipe, NgIf, MatButtonModule, MatDividerModule]
})
export class AssetDialogComponent implements OnInit {
    public readonly isImage: boolean;
    public createdByUser: UserResponse | null = null;
    public updatedByUser: UserResponse | null = null;

    constructor(
        @Inject(MAT_DIALOG_DATA) public data: AssetResponse,
        private readonly assetService: AssetService,
        private readonly userService: UserService
    ) {
        this.isImage = this.checkIfImage(data.assetPath);
    }

    ngOnInit(): void {
        this.loadUsers();
    }

    private loadUsers(): void {
        const createdByUser$ = this.userService.getUserById(this.data.createdBy);
        const updatedByUser$ = this.data.updatedBy 
            ? this.userService.getUserById(this.data.updatedBy)
            : of(null);

        forkJoin({
            createdBy: createdByUser$,
            updatedBy: updatedByUser$
        }).subscribe(result => {
            this.createdByUser = result.createdBy;
            this.updatedByUser = result.updatedBy;
        });
    }

    private checkIfImage(path: string): boolean {
        const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp'];
        return imageExtensions.some(ext => path.toLowerCase().endsWith(ext));
    }

    public downloadAsset(): void {
        this.assetService.download(this.data.id).subscribe(blob => {
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            const originalExtension = this.data.assetPath.split('.').pop() || '';
            const fileName = this.data.name.includes('.') ? this.data.name : `${this.data.name}.${originalExtension}`;
            link.download = fileName;
            link.click();
            window.URL.revokeObjectURL(url);
        });
    }
}

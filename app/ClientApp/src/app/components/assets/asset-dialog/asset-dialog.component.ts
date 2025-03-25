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
import {Constants} from '../../../core/constants';
import {DomSanitizer, SafeUrl} from '@angular/platform-browser';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';

@Component({
    selector: 'app-asset-dialog',
    templateUrl: './asset-dialog.component.html',
    styleUrls: ['./asset-dialog.component.scss'],
    standalone: true,
    imports: [MatTabsModule, MatDialogModule, DatePipe, NgIf, MatButtonModule, MatDividerModule, MatProgressSpinnerModule]
})
export class AssetDialogComponent implements OnInit {
    public readonly isImage: boolean;
    public previewUrl: SafeUrl | null = null;
    public createdByUser: UserResponse | null = null;
    public updatedByUser: UserResponse | null = null;
    public isLoadingPreview: boolean = false;

    constructor(
        @Inject(MAT_DIALOG_DATA) public data: AssetResponse,
        private readonly assetService: AssetService,
        private readonly userService: UserService,
        private readonly sanitizer: DomSanitizer
    ) {
        this.isImage = this.checkIfImage(data.assetPath);
    }

    ngOnInit(): void {
        this.loadUsers();
        if (this.isImage) {
            this.loadPreview();
        }
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

    private loadPreview(): void {
        this.isLoadingPreview = true;
        this.assetService.download(this.data.id).subscribe({
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

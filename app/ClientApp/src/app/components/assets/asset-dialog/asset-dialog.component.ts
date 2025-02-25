import {Component, Inject} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogModule} from '@angular/material/dialog';
import {AssetResponse} from '../../../core/models/asset.interface';
import {MatTabsModule} from '@angular/material/tabs';
import {DatePipe} from '@angular/common';

@Component({
    selector: 'app-asset-dialog',
    template: `
    <h2 mat-dialog-title>{{data.name}}</h2>
    <mat-tab-group>
      <mat-tab label="Preview">
        <mat-dialog-content>

        </mat-dialog-content>
      </mat-tab>
      <mat-tab label="Metadata">
        <mat-dialog-content>
            <p><strong>ID:</strong> {{data.id}}</p>
            <p><strong>Name:</strong> {{data.name}}</p>
          <p><strong>Description:</strong> {{data.description}}</p>
          <p><strong>Asset Path:</strong> {{data.assetPath}}</p>
          <p><strong>Created By:</strong> {{data.createdBy}}</p>
          <p><strong>Created At:</strong> {{data.createdAt | date}}</p>
          <p><strong>Updated By:</strong> {{data.updatedBy}}</p>
          <p><strong>Updated At:</strong> {{data.updatedAt | date}}</p>
        </mat-dialog-content>
      </mat-tab>
    </mat-tab-group>
    <mat-dialog-actions>
      <button mat-raised-button mat-dialog-close class="close-button" color="primary">Close</button>
    </mat-dialog-actions>
  `,
    standalone: true,
    imports: [MatTabsModule, MatDialogModule, DatePipe]
})
export class AssetDialogComponent {
    constructor(@Inject(MAT_DIALOG_DATA) public data: AssetResponse) {
    }
}

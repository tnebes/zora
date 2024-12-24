import {Component, Inject} from '@angular/core';
import {MAT_DIALOG_DATA} from '@angular/material/dialog';

interface DialogData {
    title?: string;
    message: string;
    type?: 'information' | 'warning';
    buttonText?: string;
}

@Component({
    selector: 'app-notification-dialog',
    templateUrl: './notification-dialog.component.html',
    styleUrls: ['./notification-dialog.component.css']
})
export class NotificationDialogComponent {
    title: string;
    message: string;
    type: 'information' | 'warning';
    buttonText: string;

    constructor(@Inject(MAT_DIALOG_DATA) public data: DialogData) {
        this.title = data.title || 'Notification';
        this.message = data.message;
        this.type = data.type || 'information';
        this.buttonText = data.buttonText || 'OK';
    }
}

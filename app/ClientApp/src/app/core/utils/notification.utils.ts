import { MatDialog } from '@angular/material/dialog';
import { NotificationDialogComponent } from '../../shared/components/notification-dialog/notification-dialog.component';
import { Constants } from '../constants';

export class NotificationUtils {
    static showSuccess(dialog: MatDialog, message: string, title: string = 'Success'): void {
        NotificationUtils.showNotification(dialog, title, message, 'information');
    }

    static showError(dialog: MatDialog, message: string, error?: any, title: string = 'Error'): void {
        const errorMessage = error?.message ? `${message}: ${error.message}` : message;
        NotificationUtils.showNotification(dialog, title, errorMessage, 'warning');
    }

    static showWarning(dialog: MatDialog, message: string, title: string = 'Warning'): void {
        NotificationUtils.showNotification(dialog, title, message, 'warning');
    }

    static showInfo(dialog: MatDialog, message: string, title: string = 'Information'): void {
        NotificationUtils.showNotification(dialog, title, message, 'information');
    }

    private static showNotification(
        dialog: MatDialog, 
        title: string, 
        message: string, 
        type: 'information' | 'warning'
    ): void {
        dialog.open(NotificationDialogComponent, {
            width: Constants.DIALOG_WIDTH,
            data: {
                title,
                message,
                type
            }
        });
    }
} 
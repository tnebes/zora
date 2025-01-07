export interface SelectOption {
    value: number;
    display: string;
}

export interface OptionSource {
    id: number;
    name: string;
}

export class FormUtils {
    public static toOptions(items: OptionSource[]): SelectOption[] {
        return items.map(item => ({
            value: item.id,
            display: item.name
        }));
    }
} 
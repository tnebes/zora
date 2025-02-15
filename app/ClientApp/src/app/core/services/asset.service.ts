import {Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {AssetResponse, CreateAsset, UpdateAsset, AssetResponseDto} from "../models/asset.interface";
import {QueryParams} from "../models/query-params.interface";
import {Constants} from '../constants';
import {QueryService} from './query.service';

@Injectable({
    providedIn: 'root'
})
export class AssetService {
    private readonly apiUrl: string = Constants.ASSETS;

    constructor(
        private readonly http: HttpClient,
        private readonly queryService: QueryService
    ) {
    }

    public getAssets(queryParams: QueryParams): Observable<AssetResponseDto> {
        const params: HttpParams = this.queryService.getHttpParams(queryParams);
        return this.http.get<AssetResponseDto>(this.apiUrl, {params});
    }

    public create(asset: CreateAsset): Observable<AssetResponse> {
        const formData = new FormData();
        formData.append('file', asset.asset, asset.asset.name);
        formData.append('name', asset.name);
        formData.append('description', asset.description ?? '');
        formData.append('assetPath', asset.assetPath);

        if (asset.workAssetId) {
            formData.append('workAssetId', asset.workAssetId.toString());
        }

        return this.http.post<AssetResponse>(`${this.apiUrl}`, formData);
    }

    public update(asset: UpdateAsset): Observable<AssetResponse> {
        return this.http.put<AssetResponse>(`${this.apiUrl}/${asset.id}`, asset);
    }

    public delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    public findAssetsByTerm(searchTerm: string): Observable<AssetResponseDto> {
        if (!searchTerm || searchTerm.length < 3) {
            throw new Error('Search term must be at least 3 characters long');
        }

        const params = new HttpParams().set('searchTerm', searchTerm);
        return this.http.get<AssetResponseDto>(`${Constants.ASSETS_FIND}`, {params});
    }
}

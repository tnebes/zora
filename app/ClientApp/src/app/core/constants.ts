export class Constants {
  static readonly BASE_URL: string = "https://localhost:5001/api/v1"; // TODO this should be an environment variable & should be accessible via proxy
  static readonly AUTHENTICATION = `${Constants.BASE_URL}/authentication`;
  static readonly TOKEN = `${Constants.AUTHENTICATION}/token`;
  static readonly CURRENT_USER = `${Constants.AUTHENTICATION}/current-user`; 
  static readonly REGISTRATION = `${Constants.BASE_URL}/registration`;
  static readonly AUTHORISATION = `${Constants.BASE_URL}/authorisation`; 
  static readonly IS_ADMIN = `${Constants.AUTHORISATION}/is-admin`;  
  static readonly AUTHENTICATION_CHECK = `${Constants.AUTHENTICATION}/check`;
  static readonly JWT_TOKEN_KEY = "zora_jwt_token";
}

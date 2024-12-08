export class Constants {
  static readonly BASE_URL: string = "https://localhost:5001/api/v1"; // TODO this should be an environment variable & should be accessible via proxy
  static readonly AUTHENTICATION = `${Constants.BASE_URL}/authentication`;
  static readonly REGISTRATION = `${Constants.BASE_URL}/registration`;
  static readonly TOKEN = `${Constants.AUTHENTICATION}/token`;
  static readonly AUTHENTICATION_CHECK = `${Constants.AUTHENTICATION}/check`;
  static readonly JWT_TOKEN_KEY = "zora_jwt_token";
}

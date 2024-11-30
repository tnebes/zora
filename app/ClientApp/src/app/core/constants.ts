export class Constants {
  static readonly BASE_URL: string = "http://localhost:5000/api/v1"; // TODO this should be an environment variable & should be accessible via proxy
  static readonly API = {
    AUTHENTICATION: `${Constants.BASE_URL}/authentication`,
    REGISTRATION: `${Constants.BASE_URL}/registration`,
    TOKEN: `${Constants.BASE_URL}/authentication/token`,
    JWT_TOKEN_KEY: "zora_jwt_token"
  }
}

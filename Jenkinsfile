pipeline {
    agent any

    environment {
        DOTNET_ROOT = "/usr/bin/dotnet"
        ASPNETCORE_ENVIRONMENT = "Production"
    }

    stages {
        stage('Setup') {
            parallel {
                stage('Checkout Code') {
                    steps {
                        git branch: 'master', url: 'https://github.com/tnebes/zora.git'
                    }
                }

                stage('Stop Services') {
                    steps {
                        sh 'sudo systemctl stop zora.service'
                        sh 'sudo systemctl stop nginx'
                    }
                }
            }
        }

        stage('Restore Backend Dependencies') {
            steps {
                sh 'cd app && dotnet restore'
            }
        }

        stage('Build and Publish') {
            parallel {
                stage('Build and Publish Backend') {
                    steps {
                        sh 'cd app && dotnet publish -c Release -o ../publish'
                    }
                }

                stage('Build and Publish Frontend') {
                    steps {
                        sh 'cd app/ClientApp && npm install && npm run build -- --omit=dev'
                    }
                }
            }
        }

        stage('Deploy') {
            steps {
                sh 'rm -rf /var/www/zora/app/*'
                sh 'cp -r publish/. /var/www/zora/app/'
                sh 'cp -r publish/ClientApp/dist/* /var/www/zora/wwwroot/'
                sh 'sudo systemctl restart zora.service'
                sh 'sudo systemctl restart nginx'
            }
        }
    }

    post {
        always {
            cleanWs()
        }
    }
}

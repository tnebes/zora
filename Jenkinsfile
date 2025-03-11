pipeline {
    agent any

    environment {
        DOTNET_ROOT = "/usr/bin/dotnet"
        ASPNETCORE_ENVIRONMENT = "Production"
        SOLUTION_DIR = "app"
        PROJECT_DIR = "${SOLUTION_DIR}/src"
        TESTS_DIR = "${SOLUTION_DIR}/tests/zora.Tests"
        PUBLISH_DIR = "publish"
        DEPLOY_DIR = "/var/www/zora"
        FRONTEND_SRC = "${SOLUTION_DIR}/ClientApp/dist"
        FRONTEND_DEST = "${DEPLOY_DIR}/wwwroot"
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Stop Services') {
            steps {
                sh 'sudo systemctl stop zora.service || true'
                sh 'sudo systemctl stop nginx || true'
            }
        }

        stage('Restore Dependencies') {
            steps {
                sh '''
                    cd ${SOLUTION_DIR}
                    dotnet restore
                '''
            }
        }

        stage('Build') {
            steps {
                sh '''
                    cd ${SOLUTION_DIR}
                    dotnet build --configuration Release --no-restore
                '''
            }
        }

        stage('Publish') {
            steps {
                sh '''
                    cd ${PROJECT_DIR}
                    dotnet publish zora.csproj --configuration Release --no-build -o ../../${PUBLISH_DIR}
                '''
            }
        }

        stage('Deploy') {
            parallel {
                stage('Deploy Backend') {
                    steps {
                        sh '''
                            set -e  # Exit immediately if a command exits with a non-zero status
                            echo "Starting backend deployment..."
                            
                            if [ ! -d "${PUBLISH_DIR}" ]; then
                                echo "ERROR: Publish directory not found at ${PUBLISH_DIR}"
                                exit 1
                            fi
                            
                            rm -rf ${DEPLOY_DIR}/app/*
                            cp -r ${PUBLISH_DIR}/. ${DEPLOY_DIR}/app/
                            
                            if [ $? -ne 0 ]; then
                                echo "ERROR: Failed to copy backend files to deployment directory"
                                exit 1
                            fi
                            
                            echo "Backend deployment completed successfully"
                        '''
                    }
                }
                
                stage('Deploy Frontend') {
                    steps {
                        sh '''
                            set -e  # Exit immediately if a command exits with a non-zero status
                            echo "Starting frontend deployment..."
                            
                            rm -rf ${FRONTEND_DEST}/*
                            mkdir -p ${FRONTEND_DEST}
                            
                            if [ -d "${FRONTEND_SRC}" ]; then
                                echo "Copying Angular dist files to wwwroot..."
                                cp -r ${FRONTEND_SRC}/* ${FRONTEND_DEST}/
                                
                                if [ $? -ne 0 ]; then
                                    echo "ERROR: Failed to copy frontend files to wwwroot"
                                    exit 1
                                fi
                                
                                echo "Frontend deployment completed successfully"
                            else
                                echo "ERROR: Angular dist directory not found at ${FRONTEND_SRC}"
                                echo "Frontend build may have failed or was not generated"
                                exit 1
                            fi
                        '''
                    }
                }
            }
        }

        stage('Start Services') {
            steps {
                sh '''
                    sudo systemctl start zora.service
                    sudo systemctl start nginx
                '''
            }
        }

        stage('Verify Deployment') {
            steps {
                sh '''
                    echo "Verifying zora service status..."
                    systemctl is-active --quiet zora.service && echo "Zora service is running successfully" || (echo "Zora service failed to start" && exit 1)
                    
                    echo "Verifying nginx status..."
                    systemctl is-active --quiet nginx && echo "Nginx is running successfully" || (echo "Nginx failed to start" && exit 1)
                '''
            }
        }
    }

    post {
        // always {
        //     cleanWs()
        // }
        success {
            echo 'Deployment completed successfully!'
        }
        failure {
            echo 'Deployment failed!'
            sh 'sudo systemctl status zora.service || true'
            sh 'sudo systemctl status nginx || true'
            sh 'tail -n 100 /var/log/zora/app.log || true'
        }
    }
}

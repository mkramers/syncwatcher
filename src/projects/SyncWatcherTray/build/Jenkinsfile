pipeline {
    agent any
    stages {
        stage('Build') {
            steps {			
                echo 'Building..'			
								
				script {							
					result = bat (
						script: '@git describe --tags --long --dirty --always',
						returnStdout: true
					).trim()
									
					dir("./Solutions/build")
					{
						bat "powershell -ExecutionPolicy Bypass -File ./build.ps1 -buildNumber=${env.BUILD_NUMBER} -gitVersion=${result} -gitBranch=${env.BRANCH_NAME}"
					}
				}
            }
        }
        stage('Deploy') {
            steps {
                echo 'Deploying....'				
				dir("./Solutions/build")
				{
					archiveArtifacts artifacts: 'publish/*', fingerprint: true
				}				
				dir("./Projects/FusionBx/build")
				{
					archiveArtifacts artifacts: 'publish/*', fingerprint: true
				}				
				dir("./Projects/FusionMrPro/build")
				{
					archiveArtifacts artifacts: 'publish/*', fingerprint: true
				}
				cleanWs()
												
				echo 'Scanning logs....'
				
				warnings canComputeNew: false, canResolveRelativePaths: false, categoriesPattern: '', consoleParsers: [[parserName: 'Robocopy'], [parserName: 'MSBuild'], [parserName: 'Doxygen'], [parserName: 'CodeAnalysis'], [parserName: 'Resharper InspectCode']], defaultEncoding: '', excludePattern: '', healthy: '', includePattern: '', messagesPattern: '', unHealthy: ''
            }
        }
    }
}
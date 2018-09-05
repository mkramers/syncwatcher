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
									
					dir("./src/projects/syncwatchertray/build")
					{
						bat "powershell -ExecutionPolicy Bypass -File ./build.ps1 -buildNumber=${env.BUILD_NUMBER} -gitVersion=${result} -gitBranch=${env.BRANCH_NAME} --buildconfig=Install"
					}
				}
            }
        }
        stage('Deploy') {
            steps {
                echo 'Deploying....'				
				dir("./src/projects/syncwatchertray/build")
				{
					archiveArtifacts artifacts: 'publish/*', fingerprint: true
				}				

				cleanWs()
												
				echo 'Scanning logs....'
				
				warnings canComputeNew: false, canResolveRelativePaths: false, categoriesPattern: '', consoleParsers: [[parserName: 'Robocopy'], [parserName: 'MSBuild'], [parserName: 'Doxygen'], [parserName: 'CodeAnalysis'], [parserName: 'Resharper InspectCode']],  parserConfigurations: [[parserName: 'Resharper InspectCode', pattern: 'publish\\*.inspect.xml']], defaultEncoding: '', excludePattern: '', healthy: '', includePattern: '', messagesPattern: '', unHealthy: ''
            }
        }
    }
}
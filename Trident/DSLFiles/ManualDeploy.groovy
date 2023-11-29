// Top Level folder to organize your jobs
// Choose a folder name that's meaningful and relevant to your repository
// This folder must not conflict with another folder in Jenkins.
folder('DataTransfer-Legacy-RAPJobs') {
    description('Folder containing all non-CD pipelines')
}
 
 
// Choose a job name that's meaningful and relevant to your use case. Ensure you place it in the folder you created above
pipelineJob("DataTransfer-Legacy-RAPJobs/ManualDeploy") {
    parameters {
        stringParam('rapVersion', '', 'The version of the RAP that you wish to deploy. E.g. 1.2.3.4.')
        choiceParam('Ring', ['Ring 0', 'Ring 1', 'Ring 2', 'Ring 3', 'Ring 4', 'Ring 5', 'Ring 6', 'Ring 7', 'Ring 8', 'Ring 9', 'Ring 10'], 'Target ring for RAP deployment.')
        stringParam('podName', null, 'The pod name that you wish to deploy to. E.g. ctus010000')
        stringParam('tenantIDs', null, 'The comma separated list of tenants that you wish to deploy to (no spaces). E.g. 000,012,013')
        stringParam('serviceDeskCRId', '', 'The Jira Service Desk Change Request issue key. Only needed for Rings >= 1, and if you are not approved for Standard CRs.')
        booleanParam('executeRollback', false, 'Check this box to execute a rollback deployment to the rapVersion specified.')
        booleanParam('overrideAutoPromote', false, 'Check this box to override auto promote logic and only deploy to the specified ring.')
    }
    definition {
        cpsScm {           
            scm {
                git {
                    remote{
                        // This must be the Git URL of your repository
                        name('origin')
                        url('ssh://git@git.kcura.com:7999/dtx/relativity-data-transfer-legacy-sdk.git')
                        credentials('bitbucket-repo-key')
                    }
                    branches('*/master')
                    extensions {
                        cleanAfterCheckout()
                    }
                }
            }
            //The "Trident/Jobs/ManualDeploy.groovy" path here is the path to the job definition we're creating in the next step.
            // The groovy file must be in the "Trident/Jobs" folder. Make sure the file name here aligns with the actual file you create or the jobs won't be created.
            scriptPath('Trident/Jobs/ManualDeploy.groovy')
        }
    }
}
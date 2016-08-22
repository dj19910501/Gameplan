using System.Xml;
using System.Xml.XPath;
using RevenuePlanner.Models;
using System.Text;

namespace RevenuePlanner.Helpers
{
    public class Message
    {
        public Message()
        {
            // EMPTY
        }

        #region Variable Declaration
        private string Expression = "/Messages/Field";
        #endregion

        #region Variables

        private string _ServiceUnavailableMessage;
        public string ServiceUnavailableMessage
        {
            get
            {
                return _ServiceUnavailableMessage;
            }
            set
            {
                _ServiceUnavailableMessage = value;
            }
        }

        private string _DatabaseServiceUnavailableMessage;
        public string DatabaseServiceUnavailableMessage
        {
            get
            {
                return _DatabaseServiceUnavailableMessage;
            }
            set
            {
                _DatabaseServiceUnavailableMessage = value;
            }
        }


        private string _InvalidProfileImage;
        public string InvalidProfileImage
        {
            get
            {
                return _InvalidProfileImage;
            }
            set
            {
                _InvalidProfileImage = value;
            }
        }

        private string _InvalidPassword;
        public string InvalidPassword
        {
            get
            {
                return _InvalidPassword;
            }
            set
            {
                _InvalidPassword = value;
            }
        }

        private string _InvalidLogin;
        public string InvalidLogin
        {
            get
            {
                return _InvalidLogin;
            }
            set
            {
                _InvalidLogin = value;
            }
        }  
////Added by Komal Rawal for #2107
        private string _InvalidEmailLogin;
        public string InvalidEmailLogin
        {
            get
            {
                return _InvalidEmailLogin;
            }
            set
            {
                _InvalidEmailLogin = value;
            }
        }

        private string _LockedUser;
        public string LockedUser
        {
            get
            {
                return _LockedUser;
            }
            set
            {
                _LockedUser = value;
            }
        }

        private string _ErrorOccured;
        public string ErrorOccured
        {
            get
            {
                return _ErrorOccured;
            }
            set
            {
                _ErrorOccured = value;
            }
        }

        private string _UserAdded;
        public string UserAdded
        {
            get
            {
                return _UserAdded;
            }
            set
            {
                _UserAdded = value;
            }
        }

        private string _UserEdited;
        public string UserEdited
        {
            get
            {
                return _UserEdited;
            }
            set
            {
                _UserEdited = value;
            }
        }

        private string _UserDeleted;
        public string UserDeleted
        {
            get
            {
                return _UserDeleted;
            }
            set
            {
                _UserDeleted = value;
            }
        }

        private string _UserCantDeleted;
        public string UserCantDeleted
        {
            get
            {
                return _UserCantDeleted;
            }
            set
            {
                _UserCantDeleted = value;
            }
        }

        private string _UserCantEdited;
        public string UserCantEdited
        {
            get
            {
                return _UserCantEdited;
            }
            set
            {
                _UserCantEdited = value;
            }
        }

        private string _UserDuplicate;
        public string UserDuplicate
        {
            get
            {
                return _UserDuplicate;
            }
            set
            {
                _UserDuplicate = value;
            }
        }

        private string _DeleteConfirm;
        public string DeleteConfirm
        {
            get
            {
                return _DeleteConfirm;
            }
            set
            {
                _DeleteConfirm = value;
            }
        }

        private string _ActivityMasterDeleted;
        public string ActivityMasterDeleted
        {
            get
            {
                return _ActivityMasterDeleted;
            }
            set
            {
                _ActivityMasterDeleted = value;
            }
        }

        private string _ActivityMasterDuplicate;
        public string ActivityMasterDuplicate
        {
            get
            {
                return _ActivityMasterDuplicate;
            }
            set
            {
                _ActivityMasterDuplicate = value;
            }
        }

        private string _ActivityMasterAdded;
        public string ActivityMasterAdded
        {
            get
            {
                return _ActivityMasterAdded;
            }
            set
            {
                _ActivityMasterAdded = value;
            }
        }

        private string _ActivityMasterEdited;
        public string ActivityMasterEdited
        {
            get
            {
                return _ActivityMasterEdited;
            }
            set
            {
                _ActivityMasterEdited = value;
            }
        }

        private string _UserPasswordChanged;
        public string UserPasswordChanged
        {
            get
            {
                return _UserPasswordChanged;
            }
            set
            {
                _UserPasswordChanged = value;
            }
        }

        private string _CurrentUserPasswordNotCorrect;
        public string CurrentUserPasswordNotCorrect
        {
            get
            {
                return _CurrentUserPasswordNotCorrect;
            }
            set
            {
                _CurrentUserPasswordNotCorrect = value;
            }
        }

        private string _UserPasswordDoNotMatch;
        public string UserPasswordDoNotMatch
        {
            get
            {
                return _UserPasswordDoNotMatch;
            }
            set
            {
                _UserPasswordDoNotMatch = value;
            }
        }

        private string _UserNotificationsSaved;
        public string UserNotificationsSaved
        {
            get
            {
                return _UserNotificationsSaved;
            }
            set
            {
                _UserNotificationsSaved = value;
            }
        }

        private string _DuplicateCampaignExits;
        public string DuplicateCampaignExits
        {
            get
            {
                return _DuplicateCampaignExits;
            }
            set
            {
                _DuplicateCampaignExits = value;
            }
        }

        private string _DuplicateProgramExits;
        public string DuplicateProgramExits
        {
            get
            {
                return _DuplicateProgramExits;
            }
            set
            {
                _DuplicateProgramExits = value;
            }
        }

        private string _DuplicateTacticExits;
        public string DuplicateTacticExits
        {
            get
            {
                return _DuplicateTacticExits;
            }
            set
            {
                _DuplicateTacticExits = value;
            }
        }

        private string _DuplicateLineItemExits;
        public string DuplicateLineItemExits
        {
            get
            {
                return _DuplicateLineItemExits;
            }
            set
            {
                _DuplicateLineItemExits = value;
            }
        }

        private string _DeleteCampaignDependency;
        public string DeleteCampaignDependency
        {
            get
            {
                return _DeleteCampaignDependency;
            }
            set
            {
                _DeleteCampaignDependency = value;
            }
        }

        private string _DeleteProgramDependency;
        public string DeleteProgramDependency
        {
            get
            {
                return _DeleteProgramDependency;
            }
            set
            {
                _DeleteProgramDependency = value;
            }
        }

        private string _ValidateForEmptyField;
        public string ValidateForEmptyField
        {
            get
            {
                return _ValidateForEmptyField;
            }
            set
            {
                _ValidateForEmptyField = value;
            }
        }
        private string _ValidateForValidField;
        public string ValidateForValidField
        {
            get
            {
                return _ValidateForValidField;
            }
            set
            {
                _ValidateForValidField = value;
            }
        }

        private string _DateComapreValidation;
        public string DateComapreValidation
        {
            get
            {
                return _DateComapreValidation;
            }
            set
            {
                _DateComapreValidation = value;
            }
        }

        private string _CommentSuccessfully;
        public string CommentSuccessfully
        {
            get
            {
                return _CommentSuccessfully;
            }
            set
            {
                _CommentSuccessfully = value;
            }
        }

        private string _TacticStatusSuccessfully;
        public string TacticStatusSuccessfully
        {
            get
            {
                return _TacticStatusSuccessfully;
            }
            set
            {
                _TacticStatusSuccessfully = value;
            }
        }

        private string _ProgramStatusSuccessfully;
        public string ProgramStatusSuccessfully
        {
            get
            {
                return _ProgramStatusSuccessfully;
            }
            set
            {
                _ProgramStatusSuccessfully = value;
            }
        }

        private string _CampaignStatusSuccessfully;
        public string CampaignStatusSuccessfully
        {
            get
            {
                return _CampaignStatusSuccessfully;
            }
            set
            {
                _CampaignStatusSuccessfully = value;
            }
        }

        private string _TacticStartDateCompareWithParentStartDate;
        public string TacticStartDateCompareWithParentStartDate
        {
            get
            {
                return _TacticStartDateCompareWithParentStartDate;
            }
            set
            {
                _TacticStartDateCompareWithParentStartDate = value;
            }
        }

        private string _TacticEndDateCompareWithParentEndDate;
        public string TacticEndDateCompareWithParentEndDate
        {
            get
            {
                return _TacticEndDateCompareWithParentEndDate;
            }
            set
            {
                _TacticEndDateCompareWithParentEndDate = value;
            }
        }

        private string _ProgramStartDateCompareWithParentStartDate;
        public string ProgramStartDateCompareWithParentStartDate
        {
            get
            {
                return _ProgramStartDateCompareWithParentStartDate;
            }
            set
            {
                _ProgramStartDateCompareWithParentStartDate = value;
            }
        }

        private string _ProgramEndDateCompareWithParentEndDate;
        public string ProgramEndDateCompareWithParentEndDate
        {
            get
            {
                return _ProgramEndDateCompareWithParentEndDate;
            }
            set
            {
                _ProgramEndDateCompareWithParentEndDate = value;
            }
        }
        private string _ModelSaveSuccess;
        public string ModelSaveSuccess
        {
            get
            {
                return _ModelSaveSuccess;
            }
            set
            {
                _ModelSaveSuccess = value;
            }
        }
        private string _ModelAudienceSaveSuccess;
        public string ModelAudienceSaveSuccess
        {
            get
            {
                return _ModelAudienceSaveSuccess;
            }
            set
            {
                _ModelAudienceSaveSuccess = value;
            }
        }


        private string _ModelNewTacticSaveSucess;
        public string ModelNewTacticSaveSucess
        {
            get
            {
                return _ModelNewTacticSaveSucess;
            }
            set
            {
                _ModelNewTacticSaveSucess = value;
            }
        }

        private string _ModelTacticSaveSucess;
        public string ModelTacticSaveSucess
        {
            get
            {
                return _ModelTacticSaveSucess;
            }
            set
            {
                _ModelTacticSaveSucess = value;
            }
        }

        private string _ModelTacticEditSucess;
        public string ModelTacticEditSucess
        {
            get
            {
                return _ModelTacticEditSucess;
            }
            set
            {
                _ModelTacticEditSucess = value;
            }
        }

        private string _StartDateCurrentYear;
        public string StartDateCurrentYear
        {
            get
            {
                return _StartDateCurrentYear;
            }
            set
            {
                _StartDateCurrentYear = value;
            }
        }

        private string _EndDateCurrentYear;
        public string EndDateCurrentYear
        {
            get
            {
                return _EndDateCurrentYear;
            }
            set
            {
                _EndDateCurrentYear = value;
            }
        }

        private string _NoTacticFound;
        public string NoTacticFound
        {
            get
            {
                return _NoTacticFound;
            }
            set
            {
                _NoTacticFound = value;
            }
        }

        private string _NoOpenTacticExists;
        public string NoOpenTacticExists
        {
            get
            {
                return _NoOpenTacticExists;
            }
            set
            {
                _NoOpenTacticExists = value;
            }
        }

        private string _NoTacticExistPlan;
        public string NoTacticExistPlan
        {
            get
            {
                return _NoTacticExistPlan;
            }
            set
            {
                _NoTacticExistPlan = value;
            }
        }

        private string _InvalidError;
        public string InvalidError
        {
            get
            {
                return _InvalidError;
            }
            set
            {
                _InvalidError = value;
            }
        }

        private string _ModelTacticTypeNotexist;
        public string ModelTacticTypeNotexist
        {
            get
            {
                return _ModelTacticTypeNotexist;
            }
            set
            {
                _ModelTacticTypeNotexist = value;
            }
        }
        private string _ModelPublishSuccess;
        public string ModelPublishSuccess
        {
            get
            {
                return _ModelPublishSuccess;
            }
            set
            {
                _ModelPublishSuccess = value;
            }
        }

        private string _StageNotDefined;
        public string StageNotDefined
        {
            get
            {
                return _StageNotDefined;
            }
            set
            {
                _StageNotDefined = value;
            }
        }

        private string _ValidationForMqlGreaterThanINQ;
        public string ValidationForMqlGreaterThanINQ
        {
            get
            {
                return _ValidationForMqlGreaterThanINQ;
            }
            set
            {
                _ValidationForMqlGreaterThanINQ = value;
            }
        }
        /*
                               * changed by : Nirav Shah on 31 Jan 2013
                               * Bug 19:Model - should not be able to publish a model with no tactics selected */
        private string _DeleteTacticDependency;
        public string DeleteTacticDependency
        {
            get
            {
                return _DeleteTacticDependency;
            }
            set
            {
                _DeleteTacticDependency = value;
            }
        }
        private string _ModifiedTactic;
        public string ModifiedTactic
        {
            get
            {
                return _ModifiedTactic;
            }
            set
            {
                _ModifiedTactic = value;
            }
        }

        private string _ModelDeleteSuccess;
        public string ModelDeleteSuccess
        {
            get
            {
                return _ModelDeleteSuccess;
            }
            set
            {
                _ModelDeleteSuccess = value;
            }
        }
        private string _ModelDeleteDependency;
        public string ModelDeleteDependency
        {
            get
            {
                return _ModelDeleteDependency;
            }
            set
            {
                _ModelDeleteDependency = value;
            }
        }
        private string _NoModelFound;
        public string NoModelFound
        {
            get
            {
                return _NoModelFound;
            }
            set
            {
                _NoModelFound = value;
            }
        }
        private string _ModelDeleteConfirmMessage;
        public string ModelDeleteConfirmMessage
        {
            get
            {
                return _ModelDeleteConfirmMessage;
            }
            set
            {
                _ModelDeleteConfirmMessage = value;
            }
        }

        // Start - Added by :- Sohel Pathan on 06/06/2014 for PL ticket #219 to clone a model.
        private string _ModelDuplicated;
        public string ModelDuplicated
        {
            get
            {
                return _ModelDuplicated;
            }
            set
            {
                _ModelDuplicated = value;
            }
        }
        // End - Added by :- Sohel Pathan on 06/06/2014 for PL ticket #219 to clone a model.

        // Start - Added by :- Sohel Pathan on 06/06/2014 for PL ticket #516.
        private string _TargetStageNotAssociatedWithModelMsg;
        public string TargetStageNotAssociatedWithModelMsg
        {
            get
            {
                return _TargetStageNotAssociatedWithModelMsg;
            }
            set
            {
                _TargetStageNotAssociatedWithModelMsg = value;
            }
        }
        // End - Added by :- Sohel Pathan on 06/06/2014 for PL ticket #516.

        private string _PlanDeleteSuccessful;
        public string PlanDeleteSuccessful
        {
            get
            {
                return _PlanDeleteSuccessful;
            }
            set
            {
                _PlanDeleteSuccessful = value;
            }
        }
        private string _PlanDeleteError;
        public string PlanDeleteError
        {
            get
            {
                return _PlanDeleteError;
            }
            set
            {
                _PlanDeleteError = value;
            }
        }
        private string _PlanDeleteConfirmMessage;
        public string PlanDeleteConfirmMessage
        {
            get
            {
                return _PlanDeleteConfirmMessage;
            }
            set
            {
                _PlanDeleteConfirmMessage = value;
            }
        }

        private string _ModelDeleteParentDependency;
        public string ModelDeleteParentDependency
        {
            get
            {
                return _ModelDeleteParentDependency;
            }
            set
            {
                _ModelDeleteParentDependency = value;
            }
        }
        /*added by Nirav shah on 18 feb 2014 for TFS Point :252 editing published model*/
        private string _ModelPublishCreateNew;
        public string ModelPublishCreateNew
        {
            get
            {
                return _ModelPublishCreateNew;
            }
            set
            {
                _ModelPublishCreateNew = value;
            }
        }
        private string _ModelPublishEdit;
        public string ModelPublishEdit
        {
            get
            {
                return _ModelPublishEdit;
            }
            set
            {
                _ModelPublishEdit = value;
            }
        }

        private string _ModelPublishTacticDelete;
        public string ModelPublishTacticDelete
        {
            get
            {
                return _ModelPublishTacticDelete;
            }
            set
            {
                _ModelPublishTacticDelete = value;
            }
        }
        private string _ModelPublishComfirmation;
        public string ModelPublishComfirmation
        {
            get
            {
                return _ModelPublishComfirmation;
            }
            set
            {
                _ModelPublishComfirmation = value;
            }
        }

        private string _ImprovementTacticStatusSuccessfully;
        public string ImprovementTacticStatusSuccessfully
        {
            get
            {
                return _ImprovementTacticStatusSuccessfully;
            }
            set
            {
                _ImprovementTacticStatusSuccessfully = value;
            }
        }

        private string _NewImprovementTacticSaveSucess;
        public string NewImprovementTacticSaveSucess
        {
            get
            {
                return _NewImprovementTacticSaveSucess;
            }
            set
            {
                _NewImprovementTacticSaveSucess = value;
            }
        }
        private string _EditImprovementTacticSaveSucess;
        public string EditImprovementTacticSaveSucess
        {
            get
            {
                return _EditImprovementTacticSaveSucess;
            }
            set
            {
                _EditImprovementTacticSaveSucess = value;
            }
        }
        //// Start - Added By :- Sohel Pathan on 20/04/2014 for PL #457 to delete a boost tactic
        private string _DeleteImprovementTacticSaveSucess;
        public string DeleteImprovementTacticSaveSucess
        {
            get
            {
                return _DeleteImprovementTacticSaveSucess;
            }
            set
            {
                _DeleteImprovementTacticSaveSucess = value;
            }
        }
        private string _ImprovementTacticReferencesPlanError;
        public string ImprovementTacticReferencesPlanError
        {
            get
            {
                return _ImprovementTacticReferencesPlanError;
            }
            set
            {
                _ImprovementTacticReferencesPlanError = value;
            }
        }
        //// End - Added By :- Sohel Pathan on 20/04/2014 for PL #457 to delete a boost tactic
        private string _DuplicateImprovementTacticExits;
        public string DuplicateImprovementTacticExits
        {
            get
            {
                return _DuplicateImprovementTacticExits;
            }
            set
            {
                _DuplicateImprovementTacticExits = value;
            }
        }
        private string _StageNotExist;
        public string StageNotExist
        {
            get
            {
                return _StageNotExist;
            }
            set
            {
                _StageNotExist = value;
            }
        }
        private string _EmailNotExistInDatabse;
        public string EmailNotExistInDatabse
        {
            get { return _EmailNotExistInDatabse; }
            set { _EmailNotExistInDatabse = value; }
        }

        private string _SecurityQuestionNotFound;
        public string SecurityQuestionNotFound
        {
            get { return _SecurityQuestionNotFound; }
            set { _SecurityQuestionNotFound = value; }
        }

        private string _PasswordResetLinkAlreadyUsed;
        public string PasswordResetLinkAlreadyUsed
        {
            get { return _PasswordResetLinkAlreadyUsed; }
            set { _PasswordResetLinkAlreadyUsed = value; }
        }

        private string _PasswordResetLinkExpired;

        public string PasswordResetLinkExpired
        {
            get { return _PasswordResetLinkExpired; }
            set { _PasswordResetLinkExpired = value; }
        }

        private string _AnswerNotMatched;
        public string AnswerNotMatched
        {
            get { return _AnswerNotMatched; }
            set { _AnswerNotMatched = value; }
        }

        private string _PossibleAttemptLimitExceed;
        public string PossibleAttemptLimitExceed
        {
            get { return _PossibleAttemptLimitExceed; }
            set { _PossibleAttemptLimitExceed = value; }
        }

        private string _SecurityQuestionChangesNotApplied;
        public string SecurityQuestionChangesNotApplied
        {
            get { return _SecurityQuestionChangesNotApplied; }
            set { _SecurityQuestionChangesNotApplied = value; }
        }

        private string _SecurityQuestionChangesApplied;
        public string SecurityQuestionChangesApplied
        {
            get { return _SecurityQuestionChangesApplied; }
            set { _SecurityQuestionChangesApplied = value; }
        }

        /*Added by Kuber Joshi on 11 Apr 2014 for TFS Point 220 : Ability to switch models for a plan*/
        private string _CannotSwitchModelForPlan;
        public string CannotSwitchModelForPlan
        {
            get
            {
                return _CannotSwitchModelForPlan;
            }
            set
            {
                _CannotSwitchModelForPlan = value;
            }
        }

        private string _SameImprovementTypeExits;
        public string SameImprovementTypeExits
        {
            get { return _SameImprovementTypeExits; }
            set { _SameImprovementTypeExits = value; }
        }
        //Start Manoj Limbachiya 05May2014 PL#458
        private string _ModelTacticDeleted;
        public string ModelTacticDeleted
        {
            get { return _ModelTacticDeleted; }
            set { _ModelTacticDeleted = value; }
        }
        private string _ModelTacticCannotDelete;
        public string ModelTacticCannotDelete
        {
            get { return _ModelTacticCannotDelete; }
            set { _ModelTacticCannotDelete = value; }
        }
        //End Manoj Limbachiya 05May2014 PL#458	  

        private string _ModelIntegrationSaveSuccess;
        public string ModelIntegrationSaveSuccess
        {
            get
            {
                return _ModelIntegrationSaveSuccess;
            }
            set
            {
                _ModelIntegrationSaveSuccess = value;
            }
        }

        private string _DeployedToIntegrationStatusSaveSuccess;
        public string DeployedToIntegrationStatusSaveSuccess
        {
            get
            {
                return _DeployedToIntegrationStatusSaveSuccess;
            }
            set
            {
                _DeployedToIntegrationStatusSaveSuccess = value;
            }
        }
        // Start - Added by : Sohel Pathan on 09/05/2014 for PL #430 
        private string _IntegrationAdded;
        public string IntegrationAdded
        {
            get { return _IntegrationAdded; }
            set
            {
                _IntegrationAdded = value;
            }
        }

        private string _IntegrationEdited;
        public string IntegrationEdited
        {
            get
            {
                return _IntegrationEdited;
            }
            set
            {
                _IntegrationEdited = value;
            }
        }

        private string _IntegrationDeleted;
        public string IntegrationDeleted
        {
            get
            {
                return _IntegrationDeleted;
            }
            set
            {
                _IntegrationDeleted = value;
            }
        }

        private string _IntegrationDeleteConfirmationMsg;
        public string IntegrationDeleteConfirmationMsg
        {
            get
            {
                return _IntegrationDeleteConfirmationMsg;
            }
            set
            {
                _IntegrationDeleteConfirmationMsg = value;
            }
        }

        private string _IntegrationDuplicate;
        public string IntegrationDuplicate
        {
            get
            {
                return _IntegrationDuplicate;
            }
            set
            {
                _IntegrationDuplicate = value;
            }
        }

        private string _TestIntegrationSuccess;
        public string TestIntegrationSuccess
        {
            get
            {
                return _TestIntegrationSuccess;
            }
            set
            {
                _TestIntegrationSuccess = value;
            }
        }

        private string _TestIntegrationFail;
        public string TestIntegrationFail
        {
            get
            {
                return _TestIntegrationFail;
            }
            set
            {
                _TestIntegrationFail = value;
            }
        }

        private string _IntegrationInActiveConfirmationMsg;
        public string IntegrationInActiveConfirmationMsg
        {
            get
            {
                return _IntegrationInActiveConfirmationMsg;
            }
            set
            {
                _IntegrationInActiveConfirmationMsg = value;
            }
        }

        private string _SyncNowSuccMessage;
        public string SyncNowSuccMessage
        {
            get
            {
                return _SyncNowSuccMessage;
            }
            set
            {
                _SyncNowSuccMessage = value;
            }
        }

        private string _SyncNowErrorMessage;
        public string SyncNowErrorMessage
        {
            get
            {
                return _SyncNowErrorMessage;
            }
            set
            {
                _SyncNowErrorMessage = value;
            }
        }
        // End - Added by : Sohel Pathan on 09/05/2014 for PL #430 
        private string _DataTypeMappingSaveSuccess;
        public string DataTypeMappingSaveSuccess
        {
            get
            {
                return _DataTypeMappingSaveSuccess;
            }
            set
            {
                _DataTypeMappingSaveSuccess = value;
            }
        }
        private string _DataTypeMappingNotConfigured;
        public string DataTypeMappingNotConfigured
        {
            get
            {
                return _DataTypeMappingNotConfigured;
            }
            set
            {
                _DataTypeMappingNotConfigured = value;
            }
        }

        //// Start - Added by :- Sohel Pathan on 28/05/2014 for PL #494 filter gameplan datatype by client id 
        private string _TargetFieldInvalidMsg;
        public string TargetFieldInvalidMsg
        {
            get
            {
                return _TargetFieldInvalidMsg;
            }
            set
            {
                _TargetFieldInvalidMsg = value;
            }
        }
        //// End - Added by :- Sohel Pathan on 28/05/2014 for PL #494 filter gameplan datatype by client id 

        //Start Manoj Limbachiya PL # 486
        private string _TacticReqForPublishedModel;
        public string TacticReqForPublishedModel
        {
            get
            {
                return _TacticReqForPublishedModel;
            }
            set
            {
                _TacticReqForPublishedModel = value;
            }
        }
        private string _TacticCanNotDeployed;
        public string TacticCanNotDeployed
        {
            get
            {
                return _TacticCanNotDeployed;
            }
            set
            {
                _TacticCanNotDeployed = value;
            }
        }
        //End Manoj Limbachiya PL # 486
        private string _LoginWithSameSession;
        public string LoginWithSameSession
        {
            get
            {
                return _LoginWithSameSession;
            }
            set
            {
                _LoginWithSameSession = value;
            }
        }
        // Start - Added By Sohel Pathan on 16/06/2014 for PL ticket #528
        private string _ChangeTargetStageMsg;
        public string ChangeTargetStageMsg
        {
            get
            {
                return _ChangeTargetStageMsg;
            }
            set
            {
                _ChangeTargetStageMsg = value;
            }
        }
        // End - Added By Sohel Pathan on 16/06/2014 for PL ticket #528

        // Start - Added By Sohel Pathan on 25/06/2014 for PL ticket #537
        private string _UnauthorizedCommentSection;
        public string UnauthorizedCommentSection
        {
            get
            {
                return _UnauthorizedCommentSection;
            }
            set
            {
                _UnauthorizedCommentSection = value;
            }
        }
        // End - Added By Sohel Pathan on 25/06/2014 for PL ticket #537

        // Start - Added By Sohel Pathan on 26/06/2014 for PL ticket #517
        private string _NotifyBeforeManagerDeletion;
        public string NotifyBeforeManagerDeletion
        {
            get
            {
                return _NotifyBeforeManagerDeletion;
            }
            set
            {
                _NotifyBeforeManagerDeletion = value;
            }
        }
        // End - Added By Sohel Pathan on 26/06/2014 for PL ticket #517

        // Start - Added By Mitesh Vaishnav on 04/07/2014 for PL ticket #521
        private string _UserPermissionsResetToDefault;
        public string UserPermissionsResetToDefault
        {
            get
            {
                return _UserPermissionsResetToDefault;
            }
            set
            {
                _UserPermissionsResetToDefault = value;
            }
        }
        // End - Added By Mitesh Vaishnav on 04/07/2014 for PL ticket #521

        // Start - Added By Mitesh Vaishnav on 17/07/2014 for functional review point 65
        private string _ValidateForEmptyImprovementWeight;
        public string ValidateForEmptyImprovementWeight
        {
            get
            {
                return _ValidateForEmptyImprovementWeight;
            }
            set
            {
                _ValidateForEmptyImprovementWeight = value;
            }
        }
        private string _ValidateForNumericDigitOneToFive;
        public string ValidateForNumericDigitOneToFive
        {
            get
            {
                return _ValidateForNumericDigitOneToFive;
            }
            set
            {
                _ValidateForNumericDigitOneToFive = value;
            }
        }
        private string _ValidateIntegretionCredential;
        public string ValidateIntegretionCredential
        {
            get
            {
                return _ValidateIntegretionCredential;
            }
            set
            {
                _ValidateIntegretionCredential = value;
            }
        }
        private string _SynchronizationStatus;
        public string SynchronizationStatus
        {
            get
            {
                return _SynchronizationStatus;
            }
            set
            {
                _SynchronizationStatus = value;
            }
        }
        private string _ValidateForEmptyFieldAndValid;
        public string ValidateForEmptyFieldAndValid
        {
            get
            {
                return _ValidateForEmptyFieldAndValid;
            }
            set
            {
                _ValidateForEmptyFieldAndValid = value;
            }
        }
        private string _ValidateEmailAtleastone;
        public string ValidateEmailAtleastone
        {
            get
            {
                return _ValidateEmailAtleastone;
            }
            set
            {
                _ValidateEmailAtleastone = value;
            }
        }
        private string _ValidatePaswordMustSameAsConfirm;
        public string ValidatePaswordMustSameAsConfirm
        {
            get
            {
                return _ValidatePaswordMustSameAsConfirm;
            }
            set
            {
                _ValidatePaswordMustSameAsConfirm = value;
            }
        }
        private string _ValidatePassowordCannotSameAsCurrent;
        public string ValidatePassowordCannotSameAsCurrent
        {
            get
            {
                return _ValidatePassowordCannotSameAsCurrent;
            }
            set
            {
                _ValidatePassowordCannotSameAsCurrent = value;
            }
        }
        private string _HilightedFieldRequire;
        public string HilightedFieldRequire
        {
            get
            {
                return _HilightedFieldRequire;
            }
            set
            {
                _HilightedFieldRequire = value;
            }
        }
        private string _ValidatePasswordCannotSameAsOld;
        public string ValidatePasswordCannotSameAsOld
        {
            get
            {
                return _ValidatePasswordCannotSameAsOld;
            }
            set
            {
                _ValidatePasswordCannotSameAsOld = value;
            }
        }
        private string _ValidateEnteredField;
        public string ValidateEnteredField
        {
            get
            {
                return _ValidateEnteredField;
            }
            set
            {
                _ValidateEnteredField = value;
            }
        }
        private string _ModelAlreadyExits;
        public string ModelAlreadyExits
        {
            get
            {
                return _ModelAlreadyExits;
            }
            set
            {
                _ModelAlreadyExits = value;
            }
        }
        private string _ValidateConversionRateAndTargetStage;
        public string ValidateConversionRateAndTargetStage
        {
            get
            {
                return _ValidateConversionRateAndTargetStage;
            }
            set
            {
                _ValidateConversionRateAndTargetStage = value;
            }
        }
        private string _ValidateConversionRate;
        public string ValidateConversionRate
        {
            get
            {
                return _ValidateConversionRate;
            }
            set
            {
                _ValidateConversionRate = value;
            }
        }
        private string _ValidateTargetStage;
        public string ValidateTargetStage
        {
            get
            {
                return _ValidateTargetStage;
            }
            set
            {
                _ValidateTargetStage = value;
            }
        }
        private string _ConfirmationForDeleteTactic;
        public string ConfirmationForDeleteTactic
        {
            get
            {
                return _ConfirmationForDeleteTactic;
            }
            set
            {
                _ConfirmationForDeleteTactic = value;
            }
        }
        private string _RoleAlreadyExits;
        public string RoleAlreadyExits
        {
            get
            {
                return _RoleAlreadyExits;
            }
            set
            {
                _RoleAlreadyExits = value;
            }
        }
        private string _ValidateRequiredRole;
        public string ValidateRequiredRole
        {
            get
            {
                return _ValidateRequiredRole;
            }
            set
            {
                _ValidateRequiredRole = value;
            }
        }
        private string _ValidateRequiredPermission;
        public string ValidateRequiredPermission
        {
            get
            {
                return _ValidateRequiredPermission;
            }
            set
            {
                _ValidateRequiredPermission = value;
            }
        }

        private string _ValidateAtleastOneCampaign;
        public string ValidateAtleastOneCampaign
        {
            get
            {
                return _ValidateAtleastOneCampaign;
            }
            set
            {
                _ValidateAtleastOneCampaign = value;
            }
        }
        private string _AddMarketingActivitiesBeforeAddImprovementActivities;
        public string AddMarketingActivitiesBeforeAddImprovementActivities
        {
            get
            {
                return _AddMarketingActivitiesBeforeAddImprovementActivities;
            }
            set
            {
                _AddMarketingActivitiesBeforeAddImprovementActivities = value;
            }
        }
        private string _ConfirmationForDeleteCampaign;
        public string ConfirmationForDeleteCampaign
        {
            get
            {
                return _ConfirmationForDeleteCampaign;
            }
            set
            {
                _ConfirmationForDeleteCampaign = value;
            }
        }
        private string _ValidateEffectiveDate;
        public string ValidateEffectiveDate
        {
            get
            {
                return _ValidateEffectiveDate;
            }
            set
            {
                _ValidateEffectiveDate = value;
            }
        }
        private string _NoPublishPlanAvailable;
        public string NoPublishPlanAvailable
        {
            get
            {
                return _NoPublishPlanAvailable;
            }
            set
            {
                _NoPublishPlanAvailable = value;
            }
        }
        private string _NoPublishPlanAvailableOnReport;
        public string NoPublishPlanAvailableOnReport
        {
            get
            {
                return _NoPublishPlanAvailableOnReport;
            }
            set
            {
                _NoPublishPlanAvailableOnReport = value;
            }
        }
        private string _ConfirmationForDeleteProgram;
        public string ConfirmationForDeleteProgram
        {
            get
            {
                return _ConfirmationForDeleteProgram;
            }
            set
            {
                _ConfirmationForDeleteProgram = value;
            }
        }
        private string _EmailAlreadyExits;
        public string EmailAlreadyExits
        {
            get
            {
                return _EmailAlreadyExits;
            }
            set
            {
                _EmailAlreadyExits = value;
            }
        }
        private string _EmailAvailable;
        public string EmailAvailable
        {
            get
            {
                return _EmailAvailable;
            }
            set
            {
                _EmailAvailable = value;
            }
        }
        private string _RegionRequired;
        public string RegionRequired
        {
            get
            {
                return _RegionRequired;
            }
            set
            {
                _RegionRequired = value;
            }
        }
        private string _RoleRequired;
        public string RoleRequired
        {
            get
            {
                return _RoleRequired;
            }
            set
            {
                _RoleRequired = value;
            }
        }
        private string _ManagerRequired;
        public string ManagerRequired
        {
            get
            {
                return _ManagerRequired;
            }
            set
            {
                _ManagerRequired = value;
            }
        }
        private string _ReassignRequired;
        public string ReassignRequired
        {
            get
            {
                return _ReassignRequired;
            }
            set
            {
                _ReassignRequired = value;
            }
        }
        private string _NotValidEmail;
        public string NotValidEmail
        {
            get
            {
                return _NotValidEmail;
            }
            set
            {
                _NotValidEmail = value;
            }
        }
        private string _NoRecordFound;
        public string NoRecordFound
        {
            get
            {
                return _NoRecordFound;
            }
            set
            {
                _NoRecordFound = value;
            }
        }
        private string _RoleDeleteSuccess;
        public string RoleDeleteSuccess
        {
            get
            {
                return _RoleDeleteSuccess;
            }
            set
            {
                _RoleDeleteSuccess = value;
            }
        }
        private string _RoleCopySuccess;
        public string RoleCopySuccess
        {
            get
            {
                return _RoleCopySuccess;
            }
            set
            {
                _RoleCopySuccess = value;
            }
        }
        private string _ProgramDeleteSuccess;
        public string ProgramDeleteSuccess
        {
            get
            {
                return _ProgramDeleteSuccess;
            }
            set
            {
                _ProgramDeleteSuccess = value;
            }
        }
        private string _TacticDeleteSuccess;
        public string TacticDeleteSuccess
        {
            get
            {
                return _TacticDeleteSuccess;
            }
            set
            {
                _TacticDeleteSuccess = value;
            }
        }
        private string _CampaignDeleteSuccess;
        public string CampaignDeleteSuccess
        {
            get
            {
                return _CampaignDeleteSuccess;
            }
            set
            {
                _CampaignDeleteSuccess = value;
            }
        }
        private string _ImprovementTacticDeleteSuccess;
        public string ImprovementTacticDeleteSuccess
        {
            get
            {
                return _ImprovementTacticDeleteSuccess;
            }
            set
            {
                _ImprovementTacticDeleteSuccess = value;
            }
        }
        private string _CloneDuplicated;
        public string CloneDuplicated
        {
            get
            {
                return _CloneDuplicated;
            }
            set
            {
                _CloneDuplicated = value;
            }
        }
        private string _CloneAlreadyExits;
        public string CloneAlreadyExits
        {
            get
            {
                return _CloneAlreadyExits;
            }
            set
            {
                _CloneAlreadyExits = value;
            }
        }
        private string _TacticMustDeployedToModel;
        public string TacticMustDeployedToModel
        {
            get
            {
                return _TacticMustDeployedToModel;
            }
            set
            {
                _TacticMustDeployedToModel = value;
            }
        }
        //revert all the changes regarding 2115
        //private string _TacticTypeMaybeUsed;
        //public string TacticTypeMaybeUsed
        //{
        //    get
        //    {
        //        return _TacticTypeMaybeUsed;
        //    }
        //    set
        //    {
        //        _TacticTypeMaybeUsed = value;
        //    }
        //}

        private string _ConfirmationForModifyTargetIntegration;
        public string ConfirmationForModifyTargetIntegration
        {
            get
            {
                return _ConfirmationForModifyTargetIntegration;
            }
            set
            {
                _ConfirmationForModifyTargetIntegration = value;
            }
        }
        private string _MarketoSelectionValidation;
        public string MarketoSelectionValidation
        {
            get
            {
                return _MarketoSelectionValidation;
            }
            set
            {
                _MarketoSelectionValidation = value;
            }
        }
        private string _ConfirmationForDeleteImprovementTactic;
        public string ConfirmationForDeleteImprovementTactic
        {
            get
            {
                return _ConfirmationForDeleteImprovementTactic;
            }
            set
            {
                _ConfirmationForDeleteImprovementTactic = value;
            }
        }
        // End - Added By Mitesh Vaishnav on 17/07/2014 for functional review point 65
        // Start -Added By Mitesh Vaishnav on 21/07/2014 for functional review point 65
        private string _ValidateStartDate;
        public string ValidateStartDate
        {
            get
            {
                return _ValidateStartDate;
            }
            set
            {
                _ValidateStartDate = value;
            }
        }
        private string _ValidateEndDate;
        public string ValidateEndDate
        {
            get
            {
                return _ValidateEndDate;
            }
            set
            {
                _ValidateEndDate = value;
            }
        }
        private string _SessionExpired;
        public string SessionExpired
        {
            get
            {
                return _SessionExpired;
            }
            set
            {
                _SessionExpired = value;
            }
        }
        // End - Added By Mitesh Vaishnav on 21/07/2014 for functional review point 65


        //Added By : Kalpesh Sharma Functional Review Point #75
        private string _NoActiveModelFound;
        public string NoActiveModelFound
        {
            get
            {
                return _NoActiveModelFound;
            }
            set
            {
                _NoActiveModelFound = value;
            }
        }

        //Added By : Kalpesh Sharma Functional Review Point
        private string _NoPlanFoundPlanSelector;
        public string NoPlanFoundPlanSelector
        {
            get
            {
                return _NoPlanFoundPlanSelector;
            }
            set
            {
                _NoPlanFoundPlanSelector = value;
            }
        }

        private string _CannotAllocateMorethanRemainingBudgeted;
        public string CannotAllocateMorethanRemainingBudgeted
        {
            get { return _CannotAllocateMorethanRemainingBudgeted; }
            set { _CannotAllocateMorethanRemainingBudgeted = value; }
        }
        private string _CannotSetBudgetLessthanAllocated;
        public string CannotSetBudgetLessthanAllocated
        {
            get { return _CannotSetBudgetLessthanAllocated; }
            set { _CannotSetBudgetLessthanAllocated = value; }
        }
        private string _CannotAllocateMorehanBudgeted;
        public string CannotAllocateMorehanBudgeted
        {
            get { return _CannotAllocateMorehanBudgeted; }
            set { _CannotAllocateMorehanBudgeted = value; }
        }
        private string _CampaignBudgetIsLowerthanTotalAllocatedPrograms;
        public string CampaignBudgetIsLowerthanTotalAllocatedPrograms
        {
            get { return _CampaignBudgetIsLowerthanTotalAllocatedPrograms; }
            set { _CampaignBudgetIsLowerthanTotalAllocatedPrograms = value; }
        }
        //Start Added By Sohel #597 
        private string _ErrMsgLessPlanBudget;
        public string ErrMsgLessPlanBudget
        {
            get
            {
                return _ErrMsgLessPlanBudget;
            }
            set
            {
                _ErrMsgLessPlanBudget = value;
            }
        }

        private string _ErrMsgLessCampaignBudget;
        public string ErrMsgLessCampaignBudget
        {
            get
            {
                return _ErrMsgLessCampaignBudget;
            }
            set
            {
                _ErrMsgLessCampaignBudget = value;
            }
        }

        private string _ErrMsgBudgetAllocationExceeds;
        public string ErrMsgBudgetAllocationExceeds
        {
            get
            {
                return _ErrMsgBudgetAllocationExceeds;
            }
            set
            {
                _ErrMsgBudgetAllocationExceeds = value;
            }
        }

        private string _DefaultPlanAllocationMessage;
        public string DefaultPlanAllocationMessage
        {
            get
            {
                return _DefaultPlanAllocationMessage;
            }
            set
            {
                _DefaultPlanAllocationMessage = value;
            }
        }
        //End Added By Sohel #597 

        //Added By : Kalpesh Sharma :: Formatted the currency and minus sign in string  

        private string _ConfirmationForDeleteLineItem;
        public string ConfirmationForDeleteLineItem
        {
            get { return _ConfirmationForDeleteLineItem; }
            set { _ConfirmationForDeleteLineItem = value; }
        }

        private string _CloseDealTargetFieldInvalidMsg;
        public string CloseDealTargetFieldInvalidMsg
        {
            get { return _CloseDealTargetFieldInvalidMsg; }
            set { _CloseDealTargetFieldInvalidMsg = value; }
        }

        private string _DataTypeMappingPullSaveSuccess;
        public string DataTypeMappingPullSaveSuccess
        {
            get { return _DataTypeMappingPullSaveSuccess; }
            set { _DataTypeMappingPullSaveSuccess = value; }
        }
        private string _NoDataTypeMappingFieldsForEloqua;
        public string NoDataTypeMappingFieldsForEloqua
        {
            get { return _NoDataTypeMappingFieldsForEloqua; }
            set { _NoDataTypeMappingFieldsForEloqua = value; }
        }
        // Start - Added by Sohel Pathan on 21/08/2014 for PL ticket #716.
        private string _DataLoseErrorMessage;
        public string DataLoseErrorMessage
        {
            get { return _DataLoseErrorMessage; }
            set { _DataLoseErrorMessage = value; }
        }
        // End - Added by Sohel Pathan on 21/08/2014 for PL ticket #716.


        #region Start - Manoj PL#679
        private string _DuplicateFileLocation;
        public string DuplicateFileLocation
        {
            get { return _DuplicateFileLocation; }
            set { _DuplicateFileLocation = value; }
        }
        private string _ConnectionFail;
        public string ConnectionFail
        {
            get { return _ConnectionFail; }
            set { _ConnectionFail = value; }
        }
        private string _ServerConfigurationSaved;
        public string ServerConfigurationSaved
        {
            get { return _ServerConfigurationSaved; }
            set { _ServerConfigurationSaved = value; }
        }
        #endregion
        //Added by Mitesh for PL ticket #559
        private string _IntegrationSelectionSaved;
        public string IntegrationSelectionSaved
        {
            get { return _IntegrationSelectionSaved; }
            set { _IntegrationSelectionSaved = value; }
        }
        //End :Added by Mitesh for PL ticket #559
        //Added by Mitesh for PL ticket #752
        private string _CannotAllocateLessThanPlanned;
        public string CannotAllocateLessThanPlanned
        {
            get { return _CannotAllocateLessThanPlanned; }
            set { _CannotAllocateLessThanPlanned = value; }
        }
        //End :Added by Mitesh for PL ticket #752

        //Added by Pratik for PL ticket #754
        private string _StagesConfigurationMissMatch;
        public string StagesConfigurationMissMatch
        {
            get { return _StagesConfigurationMissMatch; }
            set { _StagesConfigurationMissMatch = value; }
        }
        //End - Added by Pratik for PL ticket #754

        // Start - Added by Sohel Pathan on 12/11/2014 for PL ticket #933
        private string _ChangesSaved;
        public string ChangesSaved
        {
            get { return _ChangesSaved; }
            set { _ChangesSaved = value; }
        }
        // End - Added by Sohel Pathan on 12/11/2014 for PL ticket #933
        // Start - Added by Viral Kadiya on 17/11/2014 for PL ticket #947.

        // Start - Added by Viral Kadiya on 17/11/2014 for PL ticket #947.
        private string _PlanEntityCreated;
        public string PlanEntityCreated
        {
            get { return _PlanEntityCreated; }
            set { _PlanEntityCreated = value; }
        }
        private string _PlanEntityUpdated;
        public string PlanEntityUpdated
        {
            get { return _PlanEntityUpdated; }
            set { _PlanEntityUpdated = value; }
        }
        private string _PlanEntityDeclined;
        public string PlanEntityDeclined
        {
            get { return _PlanEntityDeclined; }
            set { _PlanEntityDeclined = value; }
        }
        private string _PlanEntitySubmittedForApproval;
        public string PlanEntitySubmittedForApproval
        {
            get { return _PlanEntitySubmittedForApproval; }
            set { _PlanEntitySubmittedForApproval = value; }
        }
        private string _PlanEntityApproved;
        public string PlanEntityApproved
        {
            get { return _PlanEntityApproved; }
            set { _PlanEntityApproved = value; }
        }
        private string _PlanEntityAllocationUpdated;
        public string PlanEntityAllocationUpdated
        {
            get { return _PlanEntityAllocationUpdated; }
            set { _PlanEntityAllocationUpdated = value; }
        }
        private string _PlanEntityActualsUpdated;
        public string PlanEntityActualsUpdated
        {
            get { return _PlanEntityActualsUpdated; }
            set { _PlanEntityActualsUpdated = value; }
        }
        private string _PlanEntityDeleted;
        public string PlanEntityDeleted
        {
            get { return _PlanEntityDeleted; }
            set { _PlanEntityDeleted = value; }
        }
        private string _EmptyFieldValidation;
        public string EmptyFieldValidation
        {
            get { return _EmptyFieldValidation; }
            set { _EmptyFieldValidation = value; }
        }
        private string _EmptyFieldCommentAdded;
        public string EmptyFieldCommentAdded
        {
            get { return _EmptyFieldCommentAdded; }
            set { _EmptyFieldCommentAdded = value; }
        }
        private string _PlanEntityDuplicated;
        public string PlanEntityDuplicated
        {
            get { return _PlanEntityDuplicated; }
            set { _PlanEntityDuplicated = value; }
        }

        // End - Added by Viral Kadiya on 17/11/2014 for PL ticket #947.

        private string _DataInconsistency;
        public string DataInconsistency
        {
            get { return _DataInconsistency; }
            set { _DataInconsistency = value; }
        }
        //// Start - Ticket #994, by Pratik chauhan
        private string _InvalidCharacterofDescription;
        public string InvalidCharacterofDescription
        {
            get { return _InvalidCharacterofDescription; }
            set { _InvalidCharacterofDescription = value; }
        }

        //// End - Ticket #994, by Pratik chauhan

        //// Start - Ticket #998, by Pratik chauhan
        private string _IntegrationFolderPathSaved;

        public string IntegrationFolderPathSaved
        {
            get { return _IntegrationFolderPathSaved; }
            set { _IntegrationFolderPathSaved = value; }
        }

        //// End - Ticket #998, by Pratik chauhan

        //// Start - Ticket #2220, by Nishant Sheth
        private string _MarketoCampaignSaved;

        public string MarketoCampaignSaved
        {
            get { return _MarketoCampaignSaved; }
            set { _MarketoCampaignSaved = value; }
        }

        //// End - Ticket #2220, by Nishant Sheth

        // Start - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021
        private string _CrossClientLoginForInspectPopup;

        public string CrossClientLoginForInspectPopup
        {
            get { return _CrossClientLoginForInspectPopup; }
            set { _CrossClientLoginForInspectPopup = value; }
        }

        private string _DeletedEntityForInspectPopup;

        public string DeletedEntityForInspectPopup
        {
            get { return _DeletedEntityForInspectPopup; }
            set { _DeletedEntityForInspectPopup = value; }
        }

        private string _CustomRestrictionFailedForInspectPopup;

        public string CustomRestrictionFailedForInspectPopup
        {
            get { return _CustomRestrictionFailedForInspectPopup; }
            set { _CustomRestrictionFailedForInspectPopup = value; }
        }

        private string _FakeEntityForInspectPopup;

        public string FakeEntityForInspectPopup
        {
            get { return _FakeEntityForInspectPopup; }
            set { _FakeEntityForInspectPopup = value; }
        }

        private string _InvalidURLForInspectPopup;

        public string InvalidURLForInspectPopup
        {
            get { return _InvalidURLForInspectPopup; }
            set { _InvalidURLForInspectPopup = value; }
        }
        // End - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021

        //// Start - Added by :- Sohel Pathan on 23/12/2014 for PL #1061 
        private string _PullingTargetFieldInvalidMsg;
        public string PullingTargetFieldInvalidMsg
        {
            get
            {
                return _PullingTargetFieldInvalidMsg;
            }
            set
            {
                _PullingTargetFieldInvalidMsg = value;
            }
        }

        private string _DataTypeMappingPullMQLSaveSuccess;
        public string DataTypeMappingPullMQLSaveSuccess
        {
            get { return _DataTypeMappingPullMQLSaveSuccess; }
            set { _DataTypeMappingPullMQLSaveSuccess = value; }
        }
        //// End - Added by :- Sohel Pathan on 23/12/2014 for PL #1061

        ////Start - Added by Mitesh Vaishnav for PL ticket #1124
        private string _ValidateAttributeWeightSum;
        public string ValidateAttributeWeightSum
        {
            get { return _ValidateAttributeWeightSum; }
            set { _ValidateAttributeWeightSum = value; }
        }
        ////End - Added by Mitesh Vaishnav for PL ticket #1124

        ////Start - Added by Viral Kadiya for Internal Review Point
        private string _PlanSaved;
        public string PlanSaved
        {
            get
            {
                return _PlanSaved;
            }
            set
            {
                _PlanSaved = value;
            }
        }
        private string _LastSynced;
        public string LastSynced
        {
            get
            {
                return _LastSynced;
            }
            set
            {
                _LastSynced = value;
            }
        }
        ////End - Added by Viral Kadiya for Internal Review Point
        ////Start - Added by Mitesh Vaishnav for PL ticket #1215
        private string _MarketingDealDescription;
        public string MarketingDealDescription
        {
            get { return _MarketingDealDescription; }
            set { _MarketingDealDescription = value; }
        }
        ////End - Added by Mitesh Vaishnav for PL ticket #1215

        ////Start - Added by Viral Kadiya for PL ticket #1220
        private string _RevenueSparklineChartHeader;
        public string RevenueSparklineChartHeader
        {
            get
            {
                return _RevenueSparklineChartHeader;
            }
            set
            {
                _RevenueSparklineChartHeader = value;
            }
        }
        ////End - Added by Viral Kadiya for PL ticket #1220
        ////Start - Added by Viral Kadiya for PL ticket #1748
        private string _CloneEntityErrorMessage;
        public string CloneEntityErrorMessage
        {
            get
            {
                return _CloneEntityErrorMessage;
            }
            set
            {
                _CloneEntityErrorMessage = value;
            }
        }
        ////End - Added by Viral Kadiya for PL ticket #1748
        ////Start - Added by Viral Kadiya for PL ticket #1748
        private string _ExceptionErrorMessage;
        public string ExceptionErrorMessage
        {
            get
            {
                return _ExceptionErrorMessage;
            }
            set
            {
                _ExceptionErrorMessage = value;
            }
        }
        ////End - Added by Viral Kadiya for PL ticket #1748
        ////Start - Added by Viral Kadiya for PL ticket #1748
        private string _CloneEntitySuccessMessage;
        public string CloneEntitySuccessMessage
        {
            get
            {
                return _CloneEntitySuccessMessage;
            }
            set
            {
                _CloneEntitySuccessMessage = value;
            }
        }
        ////End - Added by Viral Kadiya for PL ticket #1748
        ////Start - Added by Viral Kadiya for PL ticket #1748
        private string _TacticTypeConflictMessage;
        public string TacticTypeConflictMessage
        {
            get
            {
                return _TacticTypeConflictMessage;
            }
            set
            {
                _TacticTypeConflictMessage = value;
            }
        }
        ////End - Added by Viral Kadiya for PL ticket #1748
        ////Start - Added by Viral Kadiya for PL ticket #1748
        private string _ExceptionErrorMessageforLinking;
        public string ExceptionErrorMessageforLinking
        {
            get
            {
                return _ExceptionErrorMessageforLinking;
            }
            set
            {
                _ExceptionErrorMessageforLinking = value;
            }
        }
        ////Start - Added by Rahul Shah for PL ticket #1846
        private string _LinkEntitySuccessMessage;
        public string LinkEntitySuccessMessage
        {
            get
            {
                return _LinkEntitySuccessMessage;
            }
            set
            {
                _LinkEntitySuccessMessage = value;
            }
        }
        ////End - Added by Rahul Shah for PL ticket #1846
        ////Start - Added by Rahul Shah for PL ticket #1846
        private string _LinkEntityAlreadyExist;
        public string LinkEntityAlreadyExist
        {
            get
            {
                return _LinkEntityAlreadyExist;
            }
            set
            {
                _LinkEntityAlreadyExist = value;
            }
        }
        ////End - Added by Rahul Shah for PL ticket #1846
        ////Start - Added by Rahul Shah for PL ticket #1846
        private string _TacticTypeConflictMessageforLinking;
        public string TacticTypeConflictMessageforLinking
        {
            get
            {
                return _TacticTypeConflictMessageforLinking;
            }
            set
            {
                _TacticTypeConflictMessageforLinking = value;
            }
        }
        ////End - Added by Rahul Shah for PL ticket #1846

        ////Start - Added by Rahul Shah for PL ticket #1846
        private string _ModelTypeConflict;
        public string ModelTypeConflict
        {
            get
            {
                return _ModelTypeConflict;
            }
            set
            {
                _ModelTypeConflict = value;
            }
        }
        ////End - Added by Rahul Shah for PL ticket #1846

        ////Start - Added by Rahul Shah for PL ticket #1846
        private string _DifferentModel;
        public string DifferentModel
        {
            get
            {
                return _DifferentModel;
            }
            set
            {
                _DifferentModel = value;
            }
        }
        ////End - Added by Rahul Shah for PL ticket #1846
        ////Start - Added by Rahul Shah for PL ticket #1961
        private string _NoPlanforLinking;
        public string NoPlanforLinking
        {
            get
            {
                return _NoPlanforLinking;
            }
            set
            {
                _NoPlanforLinking = value;
            }
        }
        ////End - Added by Rahul Shah for PL ticket #1961
        private string _ExtendedProgram;
        public string ExtendedProgram
        {
            get
            {
                return _ExtendedProgram;
            }
            set
            {
                _ExtendedProgram = value;
            }
        }
        private string _LinkedTacticExtendedYear;
        public string LinkedTacticExtendedYear
        {
            get { return _LinkedTacticExtendedYear; }
            set { _LinkedTacticExtendedYear = value; }
        }


        ////Start - Added by Dashrath Prajapati for PL ticket #1776
        private string _CloneEntityNonSelctionErrorMessage;
        public string CloneEntityNonSelctionErrorMessage
        {
            get
            {
                return _CloneEntityNonSelctionErrorMessage;
            }
            set
            {
                _CloneEntityNonSelctionErrorMessage = value;
            }
        }
        ////End - Added by Dashrath Prajapati for PL ticket #1776
        ////Start - Added by Dashrath Prajapati for PL ticket #1776
        private string _CloneEntityMetaDataErrorMessage;
        public string CloneEntityMetaDataErrorMessage
        {
            get
            {
                return _CloneEntityMetaDataErrorMessage;
            }
            set
            {
                _CloneEntityMetaDataErrorMessage = value;
            }
        }
        ////End - Added by Dashrath Prajapati for PL ticket #1776
        ////Start - Added by Rahul Shah for PL ticket #1899
        private string _LinkEntityInformationMessage;
        public string LinkEntityInformationMessage
        {
            get
            {
                return _LinkEntityInformationMessage;
            }
            set
            {
                _LinkEntityInformationMessage = value;
            }
        }
        ////End - Added by Rahul Shah for PL ticket #1899
        //Added By Komal Rawal for #1749
        private string _SavePresetSuccess;
        public string SavePresetSuccess
        {
            get
            {
                return _SavePresetSuccess;
            }
            set
            {
                _SavePresetSuccess = value;
            }
        }

        private string _ProvidePresetName;
        public string ProvidePresetName
        {
            get
            {
                return _ProvidePresetName;
            }
            set
            {
                _ProvidePresetName = value;
            }
        }
       //End
        ////Start - Added by Viral Kadiya for PL ticket #1748
        private string _LinkedPlanEntityDuplicated;
        public string LinkedPlanEntityDuplicated
        {
            get
            {
                return _LinkedPlanEntityDuplicated;
            }
            set
            {
                _LinkedPlanEntityDuplicated = value;
            }
        }
        ////Start - Added by Viral Kadiya for PL ticket #1970
        private string _TacticPlanedCostReduce;
        public string TacticPlanedCostReduce
        {
            get
            {
                return _TacticPlanedCostReduce;
            }
            set
            {
                _TacticPlanedCostReduce = value;
            }
        }
        
        ////End - Added by Viral Kadiya for PL ticket #1970

		////Added by Komal Rawal for #2107
        private string _PasswordExpired;
        public string PasswordExpired
        {
            get
            {
                return _PasswordExpired;
            }
            set
            {
                _PasswordExpired = value;
            }
        }
        //// Start - Ticket #2276, by devanshi gandhi
        private string _TitleContainHTMLString;
        public string TitleContainHTMLString
        {
            get { return _TitleContainHTMLString; }
            set { _TitleContainHTMLString = value; }
        }

        private string _ReportNotConfigured;
        public string ReportNotConfigured
        {
            get { return _ReportNotConfigured; }
            set { _ReportNotConfigured = value; }
        }

        private string _ApiUrlNotConfigured;
        public string ApiUrlNotConfigured
        {
            get { return _ApiUrlNotConfigured; }
            set { _ApiUrlNotConfigured = value; }
        }

        private string _ErrorInWebApi;
        public string ErrorInWebApi
        {
            get { return _ErrorInWebApi; }
            set { _ErrorInWebApi = value; }
        }
        private string _ClientDimenisionNotSet;
        public string ClientDimenisionNotSet
        {
            get { return _ClientDimenisionNotSet; }
            set { _ClientDimenisionNotSet = value; }
        }        

        private string _PackageCreated;
        public string PackageCreated
        {
            get { return _PackageCreated; }
            set { _PackageCreated = value; }
        }

        private string _UnpackageSuccessful;
        public string UnpackageSuccessful
        {
            get { return _UnpackageSuccessful; }
            set { _UnpackageSuccessful = value; }
        }

        //// End - Ticket #994, by Pratik chauhan
        // added error and suucess message for media code generation #2375
        private string _SuccessMediacode;
        public string SuccessMediacode
        {
            get { return _SuccessMediacode; }
            set { _SuccessMediacode = value; }
        }
        private string _DuplicateMediacode;
        public string DuplicateMediacode
        {
            get { return _DuplicateMediacode; }
            set { _DuplicateMediacode = value; }
        }
        private string _AtLeastOneMediacode;
        public string AtLeastOneMediacode
        {
            get { return _AtLeastOneMediacode; }
            set { _AtLeastOneMediacode = value; }
        }
        private string _RequiredMediacode;
        public string RequiredMediacode
        {
            get { return _RequiredMediacode; }
            set { _RequiredMediacode = value; }
        }
        private string _UndoMediacode;
        public string UndoMediacode
        {
            get { return _UndoMediacode; }
            set { _UndoMediacode = value; }
        }
        private string _ArchiveMediacode;
        public string ArchiveMediacode
        {
            get { return _ArchiveMediacode; }
            set { _ArchiveMediacode = value; }
        }
        private string _UnarchiveMediacode;
        public string UnarchiveMediacode
        {
            get { return _UnarchiveMediacode; }
            set { _UnarchiveMediacode = value; }
        }
        //end #2375
        ////Added by mitesh vaishnav for internal review point
        private string _BudgetTitleExist;
        public string BudgetTitleExist
        {
            get
            {
                return _BudgetTitleExist;
            }
            set
            {
                _BudgetTitleExist = value;
            }
        }
        private string _DeselectAssetFromPackage;
        public string DeselectAssetFromPackage
        {
            get
            {
                return _DeselectAssetFromPackage;
            }
            set
            {
                _DeselectAssetFromPackage = value;
            }
        }
        private string _OnlyTacticsToCreatePackage;
        public string OnlyTacticsToCreatePackage
        {
            get
            {
                return _OnlyTacticsToCreatePackage;
            }
            set
            {
                _OnlyTacticsToCreatePackage = value;
            }
        }
        private string _SinglePlanTacticForPackage;
        public string SinglePlanTacticForPackage
        {
            get
            {
                return _SinglePlanTacticForPackage;
            }
            set
            {
                _SinglePlanTacticForPackage = value;
            }
        }
        private string _AtLeastOneAssetValidation;
        public string AtLeastOneAssetValidation
        {
            get
            {
                return _AtLeastOneAssetValidation;
            }
            set
            {
                _AtLeastOneAssetValidation = value;
            }
        }
        private string _MoreThanOneAssetValidation;
        public string MoreThanOneAssetValidation
        {
            get
            {
                return _MoreThanOneAssetValidation;
            }
            set
            {
                _MoreThanOneAssetValidation = value;
            }
        }
        private string _PackageUpdated;
        public string PackageUpdated
        {
            get
            {
                return _PackageUpdated;
            }
            set
            {
                _PackageUpdated = value;
            }
        }
        //Added by kausha for #2942 on 10/08/2016
        private string _CurrencySaved;
        public string CurrencySaved
        {
            get
            {
                return _CurrencySaved;
            }
            set
            {
                _CurrencySaved = value;
            }
        }
        private string _ExchangeRateSaved;
        public string ExchangeRateSaved
        {
            get
            {
                return _ExchangeRateSaved;
            }
            set
            {
                _ExchangeRateSaved = value;
            }
        }
        // added by devanshi for Alerts and Notiffications #2417
        private string _DuplicateAlertRule;
        public string DuplicateAlertRule
        {
            get
            {
                return _DuplicateAlertRule;
            }
            set
            {
                _DuplicateAlertRule = value;
            }
        }
        private string _SuccessAlertRule;
        public string SuccessAlertRule
        {
            get
            {
                return _SuccessAlertRule;
            }
            set
            {
                _SuccessAlertRule = value;
            }
        }
        private string _UpdateAlertRule;
        public string UpdateAlertRule
        {
            get
            {
                return _UpdateAlertRule;
            }
            set
            {
                _UpdateAlertRule = value;
            }
        }
        private string _DeleteAlertRule;
        public string DeleteAlertRule
        {
            get
            {
                return _DeleteAlertRule;
            }
            set
            {
                _DeleteAlertRule = value;
            }
        }
        //end
        #endregion

        #region  Functions
        public string loadMsg(string XmlFilePath)
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFilePath);

            ////Create XPathNavigator
            XPathNavigator xpathNav = null;
            xpathNav = doc.CreateNavigator();

            ////Compile the XPath expression
            XPathExpression xpathExpr = null;
            xpathExpr = xpathNav.Compile(Expression);

            ////Display the results depending on type of result
            string strOutput = null;
            string strMsgValue = null;
            string strMsgName = null;


            switch (xpathExpr.ReturnType)
            {
                case XPathResultType.String:
                    strOutput = System.Convert.ToString(xpathNav.Evaluate(xpathExpr));

                    break;
                case XPathResultType.NodeSet:
                    XPathNodeIterator nodeIter = null;
                    nodeIter = xpathNav.Select(xpathExpr);
                    int nodecount = nodeIter.Count;
                    while (nodeIter.MoveNext())
                    {
                        if (nodeIter.Current.MoveToFirstChild())
                        {
                            int i = 0;
                            do
                            {
                                if (i == 0)
                                {
                                    strMsgName = System.Convert.ToString(nodeIter.Current.Value);
                                }
                                else
                                {
                                    strMsgValue = System.Convert.ToString(nodeIter.Current.Value);
                                    switch (strMsgName)
                                    {

                                        case "ErrorOccured":
                                            _ErrorOccured = strMsgValue;
                                            break;
                                        case "UserAdded":
                                            _UserAdded = strMsgValue;
                                            break;
                                        case "UserEdited":
                                            _UserEdited = strMsgValue;
                                            break;
                                        case "UserDeleted":
                                            _UserDeleted = strMsgValue;
                                            break;
                                        case "UserCantDeleted":
                                            _UserCantDeleted = strMsgValue;
                                            break;
                                        case "UserCantEdited":
                                            _UserCantEdited = strMsgValue;
                                            break;
                                        case "DeleteConfirm":
                                            _DeleteConfirm = strMsgValue;
                                            break;
                                        case "ActivityMasterAdded":
                                            _ActivityMasterAdded = strMsgValue;
                                            break;
                                        case "ActivityMasterDeleted":
                                            _ActivityMasterDeleted = strMsgValue;
                                            break;
                                        case "ActivityMasterDuplicate":
                                            _ActivityMasterDuplicate = strMsgValue;
                                            break;
                                        case "ActivityMasterEdited":
                                            _ActivityMasterEdited = strMsgValue;
                                            break;
                                        case "UserPasswordChanged":
                                            _UserPasswordChanged = strMsgValue;
                                            break;
                                        case "UserNotificationsSaved":
                                            _UserNotificationsSaved = strMsgValue;
                                            break;
                                        case "UserDuplicate":
                                            _UserDuplicate = strMsgValue;
                                            break;
                                        case "CurrentUserPasswordNotCorrect":
                                            _CurrentUserPasswordNotCorrect = strMsgValue;
                                            break;
                                        case "UserPasswordDoNotMatch":
                                            _UserPasswordDoNotMatch = strMsgValue;
                                            break;
                                        case "InvalidLogin":
                                            _InvalidLogin = strMsgValue;
                                            break;
                                        case "InvalidEmailLogin":
                                            _InvalidEmailLogin = strMsgValue;
                                            break;
                                        case "LockedUser":
                                            _LockedUser = strMsgValue;
                                            break;
                                            
                                        case "InvalidPassword":
                                            _InvalidPassword = strMsgValue;
                                            break;
                                        case "InvalidProfileImage":
                                            _InvalidProfileImage = strMsgValue;
                                            break;
                                        case "DuplicateCampaignExits":
                                            _DuplicateCampaignExits = strMsgValue;
                                            break;
                                        case "DuplicateProgramExits":
                                            _DuplicateProgramExits = strMsgValue;
                                            break;
                                        case "DuplicateTacticExits":
                                            _DuplicateTacticExits = strMsgValue;
                                            break;
                                        case "DuplicateLineItemExits":
                                            _DuplicateLineItemExits = strMsgValue;
                                            break;
                                        case "DeleteCampaignDependency":
                                            _DeleteCampaignDependency = strMsgValue;
                                            break;
                                        case "DeleteProgramDependency":
                                            _DeleteProgramDependency = strMsgValue;
                                            break;
                                        case "ValidateForEmptyField":
                                            _ValidateForEmptyField = strMsgValue;
                                            break;
                                        case "ValidateForValidField":
                                            _ValidateForValidField = strMsgValue;
                                            break;
                                        case "DateComapreValidation":
                                            _DateComapreValidation = strMsgValue;
                                            break;
                                        case "CommentSuccessfully":
                                            _CommentSuccessfully = strMsgValue;
                                            break;
                                        case "TacticStatusSuccessfully":
                                            _TacticStatusSuccessfully = strMsgValue;
                                            break;
                                        case "TacticStartDateCompareWithParentStartDate":
                                            _TacticStartDateCompareWithParentStartDate = strMsgValue;
                                            break;
                                        case "TacticEndDateCompareWithParentEndDate":
                                            _TacticEndDateCompareWithParentEndDate = strMsgValue;
                                            break;
                                        case "ProgramStartDateCompareWithParentStartDate":
                                            _ProgramStartDateCompareWithParentStartDate = strMsgValue;
                                            break;
                                        case "ProgramEndDateCompareWithParentEndDate":
                                            _ProgramEndDateCompareWithParentEndDate = strMsgValue;
                                            break;
                                        case "ModelSaveSuccess":
                                            _ModelSaveSuccess = strMsgValue;
                                            break;
                                        case "ModelAudienceSaveSuccess":
                                            _ModelAudienceSaveSuccess = strMsgValue;
                                            break;

                                        case "ModelNewTacticSaveSucess":
                                            _ModelNewTacticSaveSucess = strMsgValue;
                                            break;
                                        case "ModelTacticSaveSucess":
                                            _ModelTacticSaveSucess = strMsgValue;
                                            break;
                                        case "ModelTacticEditSucess":
                                            _ModelTacticEditSucess = strMsgValue;
                                            break;
                                        case "StartDateCurrentYear":
                                            _StartDateCurrentYear = strMsgValue;
                                            break;
                                        case "EndDateCurrentYear":
                                            _EndDateCurrentYear = strMsgValue;
                                            break;
                                        case "NoTacticFound":
                                            _NoTacticFound = strMsgValue;
                                            break;
                                        case "NoOpenTacticExists":
                                            _NoOpenTacticExists = strMsgValue;
                                            break;
                                        case "NoTacticExistPlan":
                                            _NoTacticExistPlan = strMsgValue;
                                            break;
                                        case "InvalidError":
                                            _InvalidError = strMsgValue;
                                            break;
                                        case "ModelTacticTypeNotexist":
                                            _ModelTacticTypeNotexist = strMsgValue;
                                            break;
                                        case "ModelPublishSuccess":
                                            _ModelPublishSuccess = strMsgValue;
                                            break;
                                        case "ServiceUnavailableMessage":
                                            _ServiceUnavailableMessage = strMsgValue;
                                            break;
                                        case "DatabaseServiceUnavailableMessage":
                                            _DatabaseServiceUnavailableMessage = strMsgValue;
                                            break;
                                        case "StageNotDefined":
                                            _StageNotDefined = strMsgValue;
                                            break;
                                        case "ProgramStatusSuccessfully":
                                            _ProgramStatusSuccessfully = strMsgValue;
                                            break;
                                        case "CampaignStatusSuccessfully":
                                            _CampaignStatusSuccessfully = strMsgValue;
                                            break;
                                        case "ValidationForMqlGreaterThanINQ":
                                            _ValidationForMqlGreaterThanINQ = strMsgValue;
                                            break;
                                        case "ModelIntegrationSaveSuccess":
                                            _ModelIntegrationSaveSuccess = strMsgValue;
                                            break;
                                        case "DeployedToIntegrationStatusSaveSuccess":
                                            _DeployedToIntegrationStatusSaveSuccess = strMsgValue;
                                            break;
                                        /*
                           * changed by : Nirav Shah on 31 Jan 2013
                           * Bug 19:Model - should not be able to publish a model with no tactics selected */
                                        case "DeleteTacticDependency":
                                            _DeleteTacticDependency = strMsgValue;
                                            break;
                                        case "ModifiedTactic":
                                            _ModifiedTactic = strMsgValue;
                                            break;
                                        /* changed by : Nirav Shah on 31 Jan 2013
                                         * TFS Bug 256 : Model list - add delete option for model */
                                        case "ModelDeleteSuccess":
                                            _ModelDeleteSuccess = strMsgValue;
                                            break;
                                        case "ModelDeleteDependency":
                                            _ModelDeleteDependency = strMsgValue;
                                            break;
                                        case "NoModelFound":
                                            _NoModelFound = strMsgValue;
                                            break;
                                        case "ModelDeleteConfirmMessage":
                                            _ModelDeleteConfirmMessage = strMsgValue;
                                            break;
                                        // Start - Added by :- Sohel Pathan on 06/06/2014 for PL ticket #219 to clone a model.
                                        case "ModelDuplicated":
                                            _ModelDuplicated = strMsgValue;
                                            break;
                                        // End - Added by :- Sohel Pathan on 06/06/2014 for PL ticket #219 to clone a model.
                                        // Start - Added by :- Sohel Pathan on 06/06/2014 for PL ticket #516.
                                        case "TargetStageNotAssociatedWithModelMsg":
                                            _TargetStageNotAssociatedWithModelMsg = strMsgValue;
                                            break;
                                        // End - Added by :- Sohel Pathan on 06/06/2014 for PL ticket #516.

                                        /* 
                                         * Changed by : Juned Katariya on 18th Feb 2014
                                         */

                                        case "PlanDeleteSuccessful":
                                            _PlanDeleteSuccessful = strMsgValue;
                                            break;
                                        case "PlanDeleteError":
                                            _PlanDeleteError = strMsgValue;
                                            break;
                                        case "PlanDeleteConfirmMessage":
                                            _PlanDeleteConfirmMessage = strMsgValue;
                                            break;


                                        /* TFS Point 252: editing a published model 
                                         * Added by : Nirav shah on 18th Feb 2014
                                         */
                                        case "ModelDeleteParentDependency":
                                            _ModelDeleteParentDependency = strMsgValue;
                                            break;
                                        case "ModelPublishCreateNew":
                                            _ModelPublishCreateNew = strMsgValue;
                                            break;
                                        case "ModelPublishEdit":
                                            _ModelPublishEdit = strMsgValue;
                                            break;
                                        case "ModelPublishTacticDelete":
                                            _ModelPublishTacticDelete = strMsgValue;
                                            break;
                                        case "ModelPublishComfirmation":
                                            _ModelPublishComfirmation = strMsgValue;
                                            break;
                                        case "ImprovementTacticStatusSuccessfully":
                                            _ImprovementTacticStatusSuccessfully = strMsgValue;
                                            break;
                                        case "NewImprovementTacticSaveSucess":
                                            _NewImprovementTacticSaveSucess = strMsgValue;
                                            break;
                                        case "EditImprovementTacticSaveSucess":
                                            _EditImprovementTacticSaveSucess = strMsgValue;
                                            break;
                                        //// Start - Added By :- Sohel Pathan on 20/04/2014 for PL #457 to delete a boost tactic
                                        case "DeleteImprovementTacticSaveSucess":
                                            _DeleteImprovementTacticSaveSucess = strMsgValue;
                                            break;
                                        case "ImprovementTacticReferencesPlanError":
                                            _ImprovementTacticReferencesPlanError = strMsgValue;
                                            break;
                                        //// End -  Added By :- Sohel Pathan on 20/04/2014 for PL #457 to delete a boost tactic
                                        case "DuplicateImprovementTacticExits":
                                            _DuplicateImprovementTacticExits = strMsgValue;
                                            break;
                                        case "StageNotExist":
                                            _StageNotExist = strMsgValue;
                                            break;
                                        /*Forgot Password
                                        Added by Dharmraj Mangukiya*/
                                        case "EmailNotExistInDatabse":
                                            _EmailNotExistInDatabse = strMsgValue;
                                            break;
                                        case "SecurityQuestionNotFound":
                                            _SecurityQuestionNotFound = strMsgValue;
                                            break;
                                        case "PasswordResetLinkAlreadyUsed":
                                            _PasswordResetLinkAlreadyUsed = strMsgValue;
                                            break;
                                        case "PasswordResetLinkExpired":
                                            _PasswordResetLinkExpired = strMsgValue;
                                            break;
                                        case "AnswerNotMatched":
                                            _AnswerNotMatched = strMsgValue;
                                            break;
                                        case "PossibleAttemptLimitExceed":
                                            _PossibleAttemptLimitExceed = strMsgValue;
                                            break;
                                        case "SecurityQuestionChangesNotApplied":
                                            _SecurityQuestionChangesNotApplied = strMsgValue;
                                            break;
                                        case "SecurityQuestionChangesApplied":
                                            _SecurityQuestionChangesApplied = strMsgValue;
                                            break;
                                        /*Added by Kuber Joshi on 11 Apr 2014 for TFS Point 220 : Ability to switch models for a plan*/
                                        case "CannotSwitchModelForPlan":
                                            _CannotSwitchModelForPlan = strMsgValue;
                                            break;
                                        case "SameImprovementTypeExits":
                                            _SameImprovementTypeExits = strMsgValue;
                                            break;
                                        //Start Manoj Limbachiya 05May2014 PL#458
                                        case "ModelTacticDeleted":
                                            _ModelTacticDeleted = strMsgValue;
                                            break;
                                        case "ModelTacticCannotDelete":
                                            _ModelTacticCannotDelete = strMsgValue;
                                            break;
                                        //End Manoj Limbachiya 05May2014 PL#458

                                        // Start - Added by : Sohel Pathan on 09/05/2014 for PL #430 
                                        case "IntegrationAdded":
                                            _IntegrationAdded = strMsgValue;
                                            break;
                                        case "IntegrationEdited":
                                            _IntegrationEdited = strMsgValue;
                                            break;
                                        case "IntegrationDeleted":
                                            _IntegrationDeleted = strMsgValue;
                                            break;
                                        case "IntegrationDuplicate":
                                            _IntegrationDuplicate = strMsgValue;
                                            break;
                                        case "IntegrationDeleteConfirmationMsg":
                                            _IntegrationDeleteConfirmationMsg = strMsgValue;
                                            break;
                                        case "TestIntegrationSuccess":
                                            _TestIntegrationSuccess = strMsgValue;
                                            break;
                                        case "TestIntegrationFail":
                                            _TestIntegrationFail = strMsgValue;
                                            break;
                                        // End - Added by : Sohel Pathan on 09/05/2014 for PL #430 
                                        case "DataTypeMappingSaveSuccess":
                                            _DataTypeMappingSaveSuccess = strMsgValue;
                                            break;
                                        case "DataTypeMappingNotConfigured":
                                            _DataTypeMappingNotConfigured = strMsgValue;
                                            break;
                                        //// Start - Added by :- Sohel Pathan on 28/05/2014 for PL #494 filter gameplan datatype by client id 
                                        case "TargetFieldInvalidMsg":
                                            _TargetFieldInvalidMsg = strMsgValue;
                                            break;
                                        //// End - Added by :- Sohel Pathan on 28/05/2014 for PL #494 filter gameplan datatype by client id 
                                        case "IntegrationInActiveConfirmationMsg":
                                            _IntegrationInActiveConfirmationMsg = strMsgValue;
                                            break;
                                        case "SyncNowSuccMessage":
                                            _SyncNowSuccMessage = strMsgValue;
                                            break;
                                        case "SyncNowErrorMessage":
                                            _SyncNowErrorMessage = strMsgValue;
                                            break;
                                        //Start Manoj Limbachiya PL # 486
                                        case "TacticReqForPublishedModel":
                                            _TacticReqForPublishedModel = strMsgValue;
                                            break;
                                        case "TacticCanNotDeployed":
                                            _TacticCanNotDeployed = strMsgValue;
                                            break;
                                        //End Manoj Limbachiya PL # 486
                                        case "LoginWithSameSession":
                                            _LoginWithSameSession = strMsgValue;
                                            break;
                                        // Start - Added By Sohel Pathan on 16/06/2014 for PL ticket #528
                                        case "ChangeTargetStageMsg":
                                            _ChangeTargetStageMsg = strMsgValue;
                                            break;
                                        // End - Added By Sohel Pathan on 16/06/2014 for PL ticket #528
                                        // Start - Added By Sohel Pathan on 25/06/2014 for PL ticket #537
                                        case "UnauthorizedCommentSection":
                                            _UnauthorizedCommentSection = strMsgValue;
                                            break;
                                        // End - Added By Sohel Pathan on 25/06/2014 for PL ticket #537
                                        // Start - Added By Sohel Pathan on 25/06/2014 for PL ticket #537
                                        case "NotifyBeforeManagerDeletion":
                                            _NotifyBeforeManagerDeletion = strMsgValue;
                                            break;
                                        // End - Added By Sohel Pathan on 25/06/2014 for PL ticket #537
                                        // Start - Added By Mitesh Vaishnav on 04/07/2014 for PL ticket #521
                                        case "UserPermissionsResetToDefault":
                                            _UserPermissionsResetToDefault = strMsgValue;
                                            break;
                                        // End - Added By Mitesh Vaishnav on 04/07/2014 for PL ticket #521
                                        // Start - Added By Mitesh Vaishnav on 17/07/2014 for functional review point 65
                                        case "ValidateForEmptyImprovementWeight":
                                            _ValidateForEmptyImprovementWeight = strMsgValue;
                                            break;
                                        case "ValidateForNumericDigitOneToFive":
                                            _ValidateForNumericDigitOneToFive = strMsgValue;
                                            break;
                                        case "ValidateIntegretionCredential":
                                            _ValidateIntegretionCredential = strMsgValue;
                                            break;
                                        case "SynchronizationStatus":
                                            _SynchronizationStatus = strMsgValue;
                                            break;
                                        case "ValidateForEmptyFieldAndValid":
                                            _ValidateForEmptyFieldAndValid = strMsgValue;
                                            break;
                                        case "ValidateEmailAtleastone":
                                            _ValidateEmailAtleastone = strMsgValue;
                                            break;
                                        case "ValidatePaswordMustSameAsConfirm":
                                            _ValidatePaswordMustSameAsConfirm = strMsgValue;
                                            break;
                                        case "ValidatePassowordCannotSameAsCurrent":
                                            _ValidatePassowordCannotSameAsCurrent = strMsgValue;
                                            break;
                                        case "HilightedFieldRequire":
                                            _HilightedFieldRequire = strMsgValue;
                                            break;
                                        case "ValidatePasswordCannotSameAsOld":
                                            _ValidatePasswordCannotSameAsOld = strMsgValue;
                                            break;
                                        case "ValidateEnteredField":
                                            _ValidateEnteredField = strMsgValue;
                                            break;
                                        case "ModelAlreadyExits":
                                            _ModelAlreadyExits = strMsgValue;
                                            break;
                                        case "ValidateConversionRateAndTargetStage":
                                            _ValidateConversionRateAndTargetStage = strMsgValue;
                                            break;
                                        case "ValidateConversionRate":
                                            _ValidateConversionRate = strMsgValue;
                                            break;
                                        case "ValidateTargetStage":
                                            _ValidateTargetStage = strMsgValue;
                                            break;
                                        case "ConfirmationForDeleteTactic":
                                            _ConfirmationForDeleteTactic = strMsgValue;
                                            break;
                                        case "RoleAlreadyExits":
                                            _RoleAlreadyExits = strMsgValue;
                                            break;
                                        case "ValidateRequiredRole":
                                            _ValidateRequiredRole = strMsgValue;
                                            break;
                                        case "ValidateRequiredPermission":
                                            _ValidateRequiredPermission = strMsgValue;
                                            break;
                                        case "ValidateAtleastOneCampaign":
                                            _ValidateAtleastOneCampaign = strMsgValue;
                                            break;
                                        case "AddMarketingActivitiesBeforeAddImprovementActivities":
                                            _AddMarketingActivitiesBeforeAddImprovementActivities = strMsgValue;
                                            break;
                                        case "ConfirmationForDeleteCampaign":
                                            _ConfirmationForDeleteCampaign = strMsgValue;
                                            break;
                                        case "ValidateEffectiveDate":
                                            _ValidateEffectiveDate = strMsgValue;
                                            break;
                                        case "NoPublishPlanAvailable":
                                            _NoPublishPlanAvailable = strMsgValue;
                                            break;
                                        case "ConfirmationForDeleteProgram":
                                            _ConfirmationForDeleteProgram = strMsgValue;
                                            break;
                                        case "EmailAlreadyExits":
                                            _EmailAlreadyExits = strMsgValue;
                                            break;
                                        case "EmailAvailable":
                                            _EmailAvailable = strMsgValue;
                                            break;
                                        case "RegionRequired":
                                            _RegionRequired = strMsgValue;
                                            break;
                                        case "RoleRequired":
                                            _RoleRequired = strMsgValue;
                                            break;
                                        case "ManagerRequired":
                                            _ManagerRequired = strMsgValue;
                                            break;
                                        case "ReassignRequired":
                                            _ReassignRequired = strMsgValue;
                                            break;
                                        case "NotValidEmail":
                                            _NotValidEmail = strMsgValue;
                                            break;
                                        case "RoleDeleteSuccess":
                                            _RoleDeleteSuccess = strMsgValue;
                                            break;
                                        case "RoleCopySuccess":
                                            _RoleCopySuccess = strMsgValue;
                                            break;
                                        case "CampaignDeleteSuccess":
                                            _CampaignDeleteSuccess = strMsgValue;
                                            break;
                                        case "ProgramDeleteSuccess":
                                            _ProgramDeleteSuccess = strMsgValue;
                                            break;
                                        case "TacticDeleteSuccess":
                                            _TacticDeleteSuccess = strMsgValue;
                                            break;
                                        case "ImprovementTacticDeleteSuccess":
                                            _ImprovementTacticDeleteSuccess = strMsgValue;
                                            break;
                                        case "CloneDuplicated":
                                            _CloneDuplicated = strMsgValue;
                                            break;
                                        case "CloneAlreadyExits":
                                            _CloneAlreadyExits = strMsgValue;
                                            break;
                                        case "TacticMustDeployedToModel":
                                            _TacticMustDeployedToModel = strMsgValue;
                                            break;
                                            //revert all the changes regarding 2115
                                        //case "TacticTypeMaybeUsed":
                                        //    _TacticTypeMaybeUsed = strMsgValue;
                                        //    break;
                                        case "ConfirmationForModifyTargetIntegration":
                                            _ConfirmationForModifyTargetIntegration = strMsgValue;
                                            break;
                                        case "ConfirmationForDeleteImprovementTactic":
                                            _ConfirmationForDeleteImprovementTactic = strMsgValue;
                                            break;
                                        case "MarketoSelectionValidation":
                                            _MarketoSelectionValidation = strMsgValue;
                                            break;
                                        // End - Added By Mitesh Vaishnav on 17/07/2014 for functional review point 65
                                        // Start -Added By Mitesh Vaishnav on 21/07/2014 for functional review point 65
                                        case "ValidateStartDate":
                                            _ValidateStartDate = strMsgValue;
                                            break;
                                        case "ValidateEndDate":
                                            _ValidateEndDate = strMsgValue;
                                            break;
                                        case "SessionExpired":
                                            _SessionExpired = strMsgValue;
                                            break;
                                        // End -Added By Mitesh Vaishnav on 21/07/2014 for functional review point 65

                                        //Added By : Kalpesh Sharma Functional Review Point #75
                                        case "NoActiveModelFound":
                                            _NoActiveModelFound = strMsgValue;
                                            break;

                                        //Added By : Kalpesh Sharma Functional Review Point #75
                                        case "NoPlanFoundPlanSelector":
                                            _NoPlanFoundPlanSelector = strMsgValue;
                                            break;

                                        //Start Added By Dharmraj #567 : Budget allocation for campaign
                                        case "CannotAllocateMorethanRemainingBudgeted":
                                            _CannotAllocateMorethanRemainingBudgeted = strMsgValue;
                                            break;
                                        case "CannotSetBudgetLessthanAllocated":
                                            _CannotSetBudgetLessthanAllocated = strMsgValue;
                                            break;
                                        case "CannotAllocateMorehanBudgeted":
                                            _CannotAllocateMorehanBudgeted = strMsgValue;
                                            break;
                                        case "CampaignBudgetIsLowerthanTotalAllocatedPrograms":
                                            _CampaignBudgetIsLowerthanTotalAllocatedPrograms = strMsgValue;
                                            break;
                                        //End Added By Dharmraj #567 : Budget allocation for campaign

                                        //Start Added By Sohel #597 
                                        case "ErrMsgLessPlanBudget":
                                            _ErrMsgLessPlanBudget = strMsgValue;
                                            break;
                                        case "ErrMsgLessCampaignBudget":
                                            _ErrMsgLessCampaignBudget = strMsgValue;
                                            break;
                                        case "ErrMsgBudgetAllocationExceeds":
                                            _ErrMsgBudgetAllocationExceeds = strMsgValue;
                                            break;
                                        case "DefaultPlanAllocationMessage":
                                            _DefaultPlanAllocationMessage = strMsgValue;
                                            break;
                                        //End Added By Sohel #597 

                                        //Added By : Kalpesh Sharma :: Formatted the currency and minus sign in string  

                                        case "ConfirmationForDeleteLineItem":
                                            _ConfirmationForDeleteLineItem = strMsgValue;
                                            break;
                                        //Start Manoj PL#679
                                        case "DuplicateFileLocation":
                                            _DuplicateFileLocation = strMsgValue;
                                            break;
                                        case "ConnectionFail":
                                            _ConnectionFail = strMsgValue;
                                            break;
                                        case "ServerConfigurationSaved":
                                            _ServerConfigurationSaved = strMsgValue;
                                            break;
                                        //End Manoj PL#679	
                                        //Added by Mitesh for PL ticket #559
                                        case "IntegrationSelectionSaved":
                                            _IntegrationSelectionSaved = strMsgValue;
                                            break;
                                        //End :Added by Mitesh for PL ticket #559
                                        case "CloseDealTargetFieldInvalidMsg":
                                            _CloseDealTargetFieldInvalidMsg = strMsgValue;
                                            break;
                                        case "DataTypeMappingPullSaveSuccess":
                                            _DataTypeMappingPullSaveSuccess = strMsgValue;
                                            break;
                                        case "NoDataTypeMappingFieldsForEloqua":
                                            _NoDataTypeMappingFieldsForEloqua = strMsgValue;
                                            break;
                                        // Start - Added by Sohel Pathan on 21/08/2014 for PL ticket #716.
                                        case "DataLoseErrorMessage":
                                            _DataLoseErrorMessage = strMsgValue;
                                            break;
                                        // End - Added by Sohel Pathan on 21/08/2014 for PL ticket #716.
                                        // Start - Added by Mitesh Vaishnav on 23/09/2014 for PL ticket #752.
                                        case "CannotAllocateLessThanPlanned":
                                            _CannotAllocateLessThanPlanned = strMsgValue;
                                            break;
                                        // End - Added by Mitesh Vaishnav on 23/09/2014 for PL ticket #752.
                                        // Start - Added by Pratik on 24/09/2014 for PL ticket #754.
                                        case "StagesConfigurationMissMatch":
                                            _StagesConfigurationMissMatch = strMsgValue;
                                            break;
                                        // End - Added by Pratik on 24/09/2014 for PL ticket #754.
                                        // Start - Added by Sohel Pathan on 12/11/2014 for PL ticket #933.
                                        case "ChangesSaved":
                                            _ChangesSaved = strMsgValue;
                                            break;
                                        // End - Added by Sohel Pathan on 12/11/2014 for PL ticket #933.
                                        // Start - Added by Viral Kadiya on 17/11/2014 for PL ticket #947.
                                        case "PlanEntityCreated":
                                            _PlanEntityCreated = strMsgValue;
                                            break;
                                        case "PlanEntityUpdated":
                                            _PlanEntityUpdated = strMsgValue;
                                            break;
                                        case "PlanEntityDeclined":
                                            _PlanEntityDeclined = strMsgValue;
                                            break;
                                        case "PlanEntitySubmittedForApproval":
                                            _PlanEntitySubmittedForApproval = strMsgValue;
                                            break;
                                        case "PlanEntityApproved":
                                            _PlanEntityApproved = strMsgValue;
                                            break;
                                        case "PlanEntityAllocationUpdated":
                                            _PlanEntityAllocationUpdated = strMsgValue;
                                            break;
                                        case "PlanEntityActualsUpdated":
                                            _PlanEntityActualsUpdated = strMsgValue;
                                            break;
                                        case "PlanEntityDeleted":
                                            _PlanEntityDeleted = strMsgValue;
                                            break;
                                        case "EmptyFieldValidation":
                                            _EmptyFieldValidation = strMsgValue;
                                            break;
                                        case "EmptyFieldCommentAdded":
                                            _EmptyFieldCommentAdded = strMsgValue;
                                            break;
                                        case "PlanEntityDuplicated":
                                            _PlanEntityDuplicated = strMsgValue;
                                            break;
                                        // End - Added by Viral Kadiya on 17/11/2014 for PL ticket #947.
                                        case "DataInconsistency":
                                            _DataInconsistency = strMsgValue;
                                            break;
                                        case "NoPublishPlanAvailableOnReport":
                                            _NoPublishPlanAvailableOnReport = strMsgValue;
                                            break;
                                        case "InvalidCharacterofDescription":
                                            _InvalidCharacterofDescription = strMsgValue;
                                            break;
                                        case "IntegrationFolderPathSaved":
                                            _IntegrationFolderPathSaved = strMsgValue;
                                            break;
                                        case "MarketoCampaignSaved":
                                            _MarketoCampaignSaved = strMsgValue;
                                            break;
                                        // Start - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021
                                        case "CrossClientLoginForInspectPopup":
                                            _CrossClientLoginForInspectPopup = strMsgValue;
                                            break;
                                        case "DeletedEntityForInspectPopup":
                                            _DeletedEntityForInspectPopup = strMsgValue;
                                            break;
                                        case "FakeEntityForInspectPopup":
                                            _FakeEntityForInspectPopup = strMsgValue;
                                            break;
                                        case "CustomRestrictionFailedForInspectPopup":
                                            _CustomRestrictionFailedForInspectPopup = strMsgValue;
                                            break;
                                        case "InvalidURLForInspectPopup":
                                            _InvalidURLForInspectPopup = strMsgValue;
                                            break;
                                        // End - Added by Sohel Pathan on 11/12/2014 for PL ticket #1021
                                        //// Start - Added by :- Sohel Pathan on 23/12/2014 for PL #1061
                                        case "PullingTargetFieldInvalidMsg":
                                            _PullingTargetFieldInvalidMsg = strMsgValue;
                                            break;
                                        case "DataTypeMappingPullMQLSaveSuccess":
                                            _DataTypeMappingPullMQLSaveSuccess = strMsgValue;
                                            break;
                                        //// End - Added by :- Sohel Pathan on 23/12/2014 for PL #1061
                                        ////Start - Added by Mitesh Vaishnav for PL ticket #1124
                                        case "ValidateAttributeWeightSum":
                                            _ValidateAttributeWeightSum = strMsgValue;
                                            break;
                                        ////End - Added by Mitesh Vaishnav for PL ticket #1124
                                        ////Start - Added by Viral Kadiya for Internal Review Point
                                        case "PlanSaved":
                                            _PlanSaved = strMsgValue;
                                            break;
                                        case "LastSynced":
                                            _LastSynced = strMsgValue;
                                            break;
                                        ////End - Added by Viral Kadiya for Internal Review Point
                                        ////Start - Added by Mitesh Vaishnav for PL ticket #1215
                                        case "MarketingDealDescription":
                                            _MarketingDealDescription = strMsgValue;
                                            break;
                                        ////End - Added by Mitesh Vaishnav for PL ticket #1215
                                        ////Start - Added by Viral Kadiya for PL ticket #1220
                                        case "RevenueSparklineChartHeader":
                                            _RevenueSparklineChartHeader = strMsgValue;
                                            break;
                                        ////End - Added by Viral Kadiya for PL ticket #1220
                                        ////Start - Added by Viral Kadiya for PL ticket #1748
                                        case "CloneEntityErrorMessage":
                                            _CloneEntityErrorMessage = strMsgValue;
                                            break;
                                        ////End - Added by Viral Kadiya for PL ticket #1748
                                        ////Start - Added by Viral Kadiya for PL ticket #1748
                                        case "ExceptionErrorMessage":
                                            _ExceptionErrorMessage = strMsgValue;
                                            break;
                                        ////End - Added by Viral Kadiya for PL ticket #1748
                                        ////Start - Added by Viral Kadiya for PL ticket #1748
                                        case "CloneEntitySuccessMessage":
                                            _CloneEntitySuccessMessage = strMsgValue;
                                            break;
                                        ////End - Added by Viral Kadiya for PL ticket #1748
                                        ////Start - Added by Viral Kadiya for PL ticket #1748
                                        case "TacticTypeConflictMessage":
                                            _TacticTypeConflictMessage = strMsgValue;
                                            break;
                                        ////End - Added by Viral Kadiya for PL ticket #1748

                                        ////Start - Added by Rahul Shah for PL ticket #1846
                                        case "ExceptionErrorMessageforLinking":
                                            _ExceptionErrorMessageforLinking = strMsgValue;
                                            break;
                                        ////End - Added by Rahul Shah for PL ticket #1846
                                        ////Start - Added by Rahul Shah for PL ticket #1846
                                        case "LinkEntitySuccessMessage":
                                            _LinkEntitySuccessMessage = strMsgValue;
                                            break;
                                        ////End - Added by Rahul Shah for PL ticket #1846
                                        ////Start - Added by Rahul Shah for PL ticket #1846
                                        case "LinkEntityAlreadyExist":
                                            _LinkEntityAlreadyExist = strMsgValue;
                                            break;
                                        ////End - Added by Rahul Shah for PL ticket #1846
                                        ////Start - Added by Rahul Shah for PL ticket #1846
                                        case "TacticTypeConflictMessageforLinking":
                                            _TacticTypeConflictMessageforLinking = strMsgValue;
                                            break;
                                        ////End - Added by Rahul Shah for PL ticket #1846
                                        ////Start - Added by Rahul Shah for PL ticket #1846
                                        case "ModelTypeConflict":
                                            _ModelTypeConflict = strMsgValue;
                                            break;
                                        ////End - Added by Rahul Shah for PL ticket #1846
                                        ////Start - Added by Rahul Shah for PL ticket #1846
                                        case "DifferentModel":
                                            _DifferentModel = strMsgValue;
                                            break;
                                        ////End - Added by Rahul Shah for PL ticket #1846
                                        ////Start - Added by Rahul Shah for PL ticket #1961
                                        case "NoPlanforLinking":
                                            _NoPlanforLinking = strMsgValue;
                                            break;
                                        ////End - Added by Rahul Shah for PL ticket #1961
                                            
                                        ////Start - Added by Rahul Shah for PL ticket #1846
                                        case "LinkedTacticExtendedYear":
                                            _LinkedTacticExtendedYear = strMsgValue;
                                            break;
                                        ////End - Added by Rahul Shah for PL ticket #1846
                                            
                                        ////Start - Added by Dashrath Prajapati for PL ticket #1776
                                        case "CloneEntityNoneSelectiontMessage":
                                            _CloneEntityNonSelctionErrorMessage = strMsgValue;
                                            break;
                                        ////End - Added by Dashrath Prajapati for PL ticket #1776
                                        ////Start -  Added by Rahul Shah for PL ticket #1899
                                        case "LinkEntityInformationMessage":
                                            _LinkEntityInformationMessage = strMsgValue;
                                            break;
                                        ////End - Added by Rahul Shah for PL ticket #1899
                                        ////Start - Added by Dashrath Prajapati for PL ticket #1776
                                        case "CloneEntityMetaDataMessage":
                                            _CloneEntityMetaDataErrorMessage = strMsgValue;
                                            break;
                                        ////End - Added by Dashrath Prajapati for PL ticket #1776
                                        ////Start - Added by Komal Rawal for PL ticket #1779
                                        case "SavePresetSuccess":
                                            _SavePresetSuccess = strMsgValue;
                                            break;
                                        case "ProvidePresetName":
                                            _ProvidePresetName = strMsgValue;
                                            break;
                                        ////End - Added by Komal Rawal for PL ticket #1779
                                        ////Start - Added by Viral Kadiya for PL ticket #1849
                                        case "LinkedPlanEntityDuplicated":
                                            _LinkedPlanEntityDuplicated = strMsgValue;
                                            break;
                                        ////End - Added by Viral Kadiya for PL ticket #1849
                                        case "ExtendedProgram":
                                            _ExtendedProgram = strMsgValue;
                                            break;
                                        ////Start - Added by Viral Kadiya for PL ticket #1970
                                        case "TacticPlanedCostReduce":
                                            _TacticPlanedCostReduce = strMsgValue;
                                            break;

                                        ////End - Added by Viral Kadiya for PL ticket #1970
									    ////Added by Komal Rawal for #2107
                                        case "PasswordExpired":
                                            _PasswordExpired = strMsgValue;
                                            break;
                                        ////added by devanshi for pl ticket #2276
                                        case "TitleContainHTMLString":
                                            _TitleContainHTMLString = strMsgValue;
                                            break;
                                        case "ReportNotConfigured":
                                            _ReportNotConfigured = strMsgValue;
                                            break;
                                        case "ApiUrlNotConfigured":
                                            _ApiUrlNotConfigured = strMsgValue;
                                            break;
                                        case "ErrorInWebApi":
                                            _ErrorInWebApi = strMsgValue;
                                            break;
                                        case "ClientDimenisionNotSet":
                                            _ClientDimenisionNotSet = strMsgValue;
                                            break;
                                            //added by devanshi for #2375 : validation for media code
                                        case "DuplicateMediacode":
                                            _DuplicateMediacode = strMsgValue;
                                            break;
                                        case "SuccessMediacode":
                                            _SuccessMediacode = strMsgValue;
                                            break;
                                        case "AtLeastOneMediacode":
                                            _AtLeastOneMediacode = strMsgValue;
                                            break;
                                        case "RequiredMediacode":
                                            _RequiredMediacode = strMsgValue;
                                            break;
                                        case "UndoMediacode":
                                            _UndoMediacode = strMsgValue;
                                            break;
                                        case "ArchiveMediacode":
                                            _ArchiveMediacode=strMsgValue;
                                            break;
                                        case "UnarchiveMediacode":
                                            _UnarchiveMediacode=strMsgValue;
                                            break;
                                            //end
                                        case "PackageCreated":
                                            _PackageCreated = strMsgValue;
                                            break;
                                        case "UnpackageSuccessful":
                                            _UnpackageSuccessful = strMsgValue;
                                            break;
                                        case "BudgetTitleExist":
                                            _BudgetTitleExist = strMsgValue;
                                            break;
                                        case "DeselectAssetFromPackage":
                                            _DeselectAssetFromPackage = strMsgValue;
                                            break;
                                        case "OnlyTacticsToCreatePackage":
                                            _OnlyTacticsToCreatePackage = strMsgValue;
                                            break;
                                        case "SinglePlanTacticForPackage":
                                            _SinglePlanTacticForPackage = strMsgValue;
                                            break;
                                        case "AtLeastOneAssetValidation":
                                            _AtLeastOneAssetValidation = strMsgValue;
                                            break;
                                        case "MoreThanOneAssetValidation":
                                            _MoreThanOneAssetValidation = strMsgValue;
                                            break;
                                        case "PackageUpdated":
                                            _PackageUpdated = strMsgValue;
                                            break;
                                        case "CurrencySaved":
                                            _CurrencySaved = strMsgValue;
                                            break;
                                        case "ExchangeRateSaved":
                                            _ExchangeRateSaved = strMsgValue;
                                            break;
                                        case "DuplicateAlertRule":
                                            _DuplicateAlertRule = strMsgValue;
                                            break;
                                        case "SuccessAlertRule":
                                            _SuccessAlertRule = strMsgValue;
                                            break;
                                        case "UpdateAlertRule":
                                            _UpdateAlertRule = strMsgValue;
                                            break;
                                        case "DeleteAlertRule":
                                            _DeleteAlertRule = strMsgValue;
                                            break;

                                    }
                                }
                                i = i + 1;
                            } while (nodeIter.Current.MoveToNext());
                            nodeIter.Current.MoveToParent();
                        }
                    }

                    break;
                case XPathResultType.Error:
                    strOutput = "Error ";

                    break;
            }
            return strOutput;
        }
        #endregion

        #region "Save IntegrationInstance Log Details Function"
        public static void SaveIntegrationInstanceLogDetails(int _entityId, int? IntegrationInstanceLogId, Enums.MessageOperation MsgOprtn, string functionName, Enums.MessageLabel MsgLabel, string logMsg)
        {
            string logDescription = string.Empty, preMessage = string.Empty;
            try
            {
                if (MsgOprtn.Equals(Enums.MessageOperation.None))
                    preMessage = (MsgLabel.Equals(Enums.MessageLabel.None) ? string.Empty : MsgLabel.ToString() + " : ") + "---";   // if message operation "None" than Message prefix should be "---" ex: . 
                else
                    preMessage = (MsgLabel.Equals(Enums.MessageLabel.None) ? string.Empty : (MsgOprtn.Equals(Enums.MessageOperation.Start)) ? string.Empty : (MsgLabel.ToString() + " : ")) + MsgOprtn.ToString() + " :";

                logDescription = preMessage + " " + functionName + " : " + logMsg;
                using (MRPEntities db = new MRPEntities())
                {
                    IntegrationInstanceLogDetail objLogDetails = new IntegrationInstanceLogDetail();
                    objLogDetails.EntityId = _entityId;
                    objLogDetails.IntegrationInstanceLogId = IntegrationInstanceLogId;
                    objLogDetails.LogTime = System.DateTime.Now;
                    objLogDetails.LogDescription = logDescription;
                    db.Entry(objLogDetails).State = System.Data.EntityState.Added;
                    db.SaveChanges();
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}



﻿using System.Xml;
using System.Xml.XPath;

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
        private string _BusinessunitRequired;
        public string BusinessunitRequired
        {
            get
            {
                return _BusinessunitRequired;
            }
            set
            {
                _BusinessunitRequired = value;
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

        // Start - Added by Sohel Pathan on 18/07/2014 for PL ticket #596
        private string _CannotCreateQuickTacticMessage;
        public string CannotCreateQuickTacticMessage
        {
            get
            {
                return _CannotCreateQuickTacticMessage;
            }
            set
            {
                _CannotCreateQuickTacticMessage = value;
            }
        }
        // End - Added by Sohel Pathan on 18/07/2014 for PL ticket #596
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
                                        case "BusinessunitRequired":
                                            _BusinessunitRequired = strMsgValue;
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
                                        case "ConfirmationForModifyTargetIntegration":
                                            _ConfirmationForModifyTargetIntegration = strMsgValue;
                                            break;
                                        case "ConfirmationForDeleteImprovementTactic":
                                            _ConfirmationForDeleteImprovementTactic = strMsgValue;
                                            break;
                                        // End - Added By Mitesh Vaishnav on 17/07/2014 for functional review point 65
                                        // Start - Added by Sohel Pathan on 18/07/2014 for PL ticket #596
                                        case "CannotCreateQuickTacticMessage":
                                            _CannotCreateQuickTacticMessage = strMsgValue;
                                            break;
                                        // End - Added by Sohel Pathan on 18/07/2014 for PL ticket #596
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
    }
}
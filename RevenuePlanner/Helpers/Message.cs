using System.Xml;
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
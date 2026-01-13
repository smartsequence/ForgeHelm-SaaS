// 簡化的多語言支援系統
// Cookie 輔助函數
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}

function setCookie(name, value, days = 365) {
    const expires = new Date();
    expires.setTime(expires.getTime() + (days * 24 * 60 * 60 * 1000));
    // 使用 SameSite=Lax 以確保跨頁面導航時 Cookie 可用
    document.cookie = `${name}=${value};expires=${expires.toUTCString()};path=/;SameSite=Lax`;
}

const i18n = {
    currentLang: getCookie('lang') || 'zh-TW',
    
    translations: {
        'zh-TW': {
            // Navigation
            'Nav.Dashboard': '儀表板',
            'Nav.Assessment': '評估',
            'Nav.Report': '報告',
            'Nav.Projects': '專案',
            'Nav.Documentation': '文件',
            'Nav.Risks': '風險',
            
            // Common
            'Common.GenerateReport': '生成報告',
            'Common.Price': 'NT$2,990',
            'Common.Language': '語言',
            'Common.Chinese': '繁體中文',
            'Common.English': 'English',
            
            // Dashboard
            'Dashboard.Title': '儀表板',
            'Dashboard.Subtitle': '專案總覽與風險分析',
            'Dashboard.Projects': '專案數',
            'Dashboard.Completion': '文件完成度',
            'Dashboard.RiskLevel': '風險等級',
            'Dashboard.RiskLow': '低',
            'Dashboard.RiskMedium': '中',
            'Dashboard.RiskHigh': '高',
            'Dashboard.RiskFingerprint': '風險指紋',
            'Dashboard.RiskAnalysis': '風險指標分析',
            'Dashboard.TechRisk': '技術風險',
            'Dashboard.ComplianceRisk': '合規風險',
            'Dashboard.OperationalRisk': '營運風險',
            'Dashboard.FinancialRisk': '財務風險',
            'Dashboard.MarketRisk': '市場風險',
            'Dashboard.HumanRisk': '人力風險',
            'Dashboard.FunctionModules': '功能模組',
            'Dashboard.QuickAccess': '快速存取主要功能',
            'Dashboard.ProjectsTitle': '專案',
            'Dashboard.ProjectsDesc': '管理專案與文件',
            'Dashboard.AssessmentTitle': '評估',
            'Dashboard.AssessmentDesc': '系統風險評估問卷',
            'Dashboard.DocumentationTitle': '文件',
            'Dashboard.DocumentationDesc': '文件管理與編輯',
            'Dashboard.RisksTitle': '風險',
            'Dashboard.RisksDesc': '風險評估與監控',
            'Dashboard.TeamTitle': '團隊',
            'Dashboard.TeamDesc': '團隊協作管理',
            'Dashboard.AnalyticsTitle': '分析',
            'Dashboard.AnalyticsDesc': '數據分析報表',
            'Dashboard.SecurityTitle': '安全',
            'Dashboard.SecurityDesc': '安全設定與權限',
            'Dashboard.SettingsTitle': '設定',
            'Dashboard.SettingsDesc': '系統設定與配置',
            
            // Assessment
            'Assessment.Title': '風險評估問卷',
            'Assessment.Subtitle': '請根據您的專案現況，評估以下各項指標（0-10分）',
            'Assessment.MaturityLabel': 'M = 成熟度 (Maturity)',
            'Assessment.MaturityDesc': '：評估各項流程與能力的成熟程度，分數越高表示該領域越成熟',
            'Assessment.ProjectNameLabel': '專案名稱',
            'Assessment.ProjectNamePlaceholder': '請輸入專案名稱',
            'Assessment.SystemNameLabel': '系統名稱',
            'Assessment.SystemNamePlaceholder': '請輸入要評估的系統名稱',
            'Assessment.SystemCodeLabel': '系統代號',
            'Assessment.SystemCodePlaceholder': '請輸入系統代號（如：ADM）',
            'Assessment.EvaluatorRoleLabel': '評估人角色',
            'Assessment.EvaluatorRolePlaceholder': '請輸入或選擇評估人角色（如：PM、SA、PG等）',
            'Assessment.M1': 'M1 系統交接',
            'Assessment.M1Desc': '新人能在1個月內上手嗎？（分數越高代表上手時間越短）',
            'Assessment.M2': 'M2 需求追溯',
            'Assessment.M2Desc': '設計決策能找到依據？',
            'Assessment.M3': 'M3 變更預測',
            'Assessment.M3Desc': '改需求前能預知影響？',
            'Assessment.M4': 'M4 驗收標準',
            'Assessment.M4Desc': '團隊對「完成」理解一致？',
            'Assessment.M5': 'M5 溝通成本',
            'Assessment.M5Desc': '跨角色對齊的溝通效率如何？（分數越高代表溝通越順暢，需要會議次數越少）',
            'Assessment.OpenQ6': '問題 6. 請描述您的專案面臨的主要挑戰',
            'Assessment.OpenQ7': '問題 7. 請說明您期望改善的領域',
            'Assessment.OpenQ8': '問題 8. 請提供其他相關資訊',
            'Assessment.PlaceholderQ6': '例如：需求變更頻繁、文件不完整、團隊溝通困難、技術債務累積...',
            'Assessment.PlaceholderQ7': '例如：建立更完善的需求追溯機制、提升文件品質與完整性、優化變更管理流程、加強團隊協作效率...',
            'Assessment.PlaceholderQ8': '例如：專案規模、團隊人數、開發週期、使用的技術棧、特殊業務需求等...',
            
            // Report
            'Report.Title': '風險評估報告',
            'Report.Subtitle': '根據您的問卷結果自動生成的風險指紋分析',
            'Report.RadarTitle': 'M1-M5 分數雷達圖',
            'Report.RadarDesc': '視覺化顯示五項關鍵風險指標',
            'Report.RadarM1': 'M1 交接',
            'Report.RadarM2': 'M2 追溯',
            'Report.RadarM3': 'M3 變更',
            'Report.RadarM4': 'M4 驗收',
            'Report.RadarM5': 'M5 溝通',
            'Report.FingerprintTitle': '風險指紋總結',
            'Report.FingerprintDesc': '整體風險輪廓與建議',
            'Report.ProjectName': '專案名稱',
            'Report.SystemName': '系統名稱',
            'Report.SystemCode': '系統代號',
            'Report.EvaluatorRole': '評估人角色',
            'Report.ReportId': '報告編號',
            'Report.AssessmentTime': '評估時間',
            'Report.RiskLevelThreshold': '0–20 高、21–35 中、36–50 低',
            'Report.TotalScore': '總分',
            'Report.RiskLevel': '風險等級',
            'Report.SampleTime': '樣本時間',
            'Report.ImprovementAdvice': '風險改善建議',
            'Report.DefaultAdvice': '將根據您的問卷結果產生具體建議。',
            'Report.DeliverablesTitle': '本次評估建議之最小交付物：',
            'Report.NoData': '尚未找到問卷資料，請先完成評估問卷。',
            'Report.NoReportTitle': '尚未生成報告',
            'Report.NoReportDesc': '完成風險評估問卷並付款後，即可查看您的專屬風險評估報告。',
            'Report.GoToAssessment': '前往評估問卷',
            'Report.AdviceLow': 'M3 變更預測偏低，建議加強需求追溯與影響分析流程，將設計決策與需求來源更緊密連結。',
            'Report.AdviceNormal': '整體變更預測能力尚可，建議持續優化需求追溯與驗收標準，以進一步降低溝通成本與風險。',
            'Report.OpenQuestions': '補充說明',
            'Report.OpenQuestionsDesc': '您的詳細說明與補充資訊',
            'Report.NoOpenAnswers': '未填寫補充說明',
            'Report.DownloadPDF': '下載 PDF 報告',
            'Report.ViewTracking': '查看 3 個月追蹤',
            'Report.InviteColleague': '邀請同事 NT$2,490/人（17% 折扣）',
            'Report.AIInsights': '風險洞察',
            'Report.AIInsightsDesc': '針對 M6、M7、M8 補充說明的深度分析',
            'Report.Analyzing': '分析中...',
            'Report.TipLabel': '提示',
            'Report.RefreshHint': '可以重新整理頁面(F5)',
            'Report.AIRefreshHint': '產生替代建議（用於腦力激盪）',
            'Report.ActionPlanTitle': '30 天行動清單',
            'Report.ActionPlanDesc': '本次評估的下一步行動建議',
            'Report.DisclaimerContent': '本報告依使用者自填資料產出\n\n本報告適用於：\n‧ 軟體專案風險盤點\n‧ 系統交接與治理檢視\n不取代：\n‧ 稽核\n‧ 法規認證\n‧ 程式碼安全掃描',
            
            // Projects
            'Projects.Title': '專案管理',
            'Projects.Subtitle': '管理您的專案與文件',
            'Projects.ComingSoonTitle': '專案管理功能開發中',
            'Projects.ComingSoonDesc': '此功能正在規劃與開發中，將提供完整的專案管理、文件版本控制、以及團隊協作功能。',
            'Projects.PlannedFeatures': '規劃中的功能',
            'Projects.Feature1': '專案建立與管理',
            'Projects.Feature2': '文件版本控制與歷史記錄',
            'Projects.Feature3': '團隊協作與權限管理',
            'Projects.Feature4': '自動化文件生成與更新',
            
            // Documentation
            'Documentation.Title': '文件管理',
            'Documentation.Subtitle': '自動生成與版本控制',
            'Documentation.ComingSoonTitle': '文件管理功能開發中',
            'Documentation.ComingSoonDesc': '此功能正在規劃與開發中，將提供 AI 驅動的文件自動生成、版本控制、以及文件模板管理功能。',
            'Documentation.PlannedFeatures': '規劃中的功能',
            'Documentation.Feature1': 'AI 自動生成技術文件',
            'Documentation.Feature2': '文件版本控制與歷史記錄',
            'Documentation.Feature3': '文件模板庫與自訂範本',
            'Documentation.Feature4': '程式碼與資料庫分析整合',
            
            // Risks
            'Risks.Title': '風險管理',
            'Risks.Subtitle': '風險監控與分析',
            'Risks.ComingSoonTitle': '風險管理功能開發中',
            'Risks.ComingSoonDesc': '此功能正在規劃與開發中，將提供完整的風險監控、趨勢分析、以及風險預警功能。',
            'Risks.PlannedFeatures': '規劃中的功能',
            'Risks.Feature1': '風險指標即時監控',
            'Risks.Feature2': '風險趨勢分析與預測',
            'Risks.Feature3': '風險預警與通知機制',
            'Risks.Feature4': '風險改善追蹤與報告',
            
            // Team
            'Team.Title': '團隊管理',
            'Team.Subtitle': '團隊協作與權限管理',
            'Team.ComingSoonTitle': '團隊管理功能開發中',
            'Team.ComingSoonDesc': '此功能正在規劃與開發中，將提供完整的團隊協作、成員管理、以及權限控制功能。',
            'Team.PlannedFeatures': '規劃中的功能',
            'Team.Feature1': '團隊成員管理與邀請',
            'Team.Feature2': '角色與權限管理',
            'Team.Feature3': '協作工作區與專案分配',
            'Team.Feature4': '團隊活動記錄與通知',
            
            // Analytics
            'Analytics.Title': '數據分析',
            'Analytics.Subtitle': '數據分析與報表',
            'Analytics.ComingSoonTitle': '數據分析功能開發中',
            'Analytics.ComingSoonDesc': '此功能正在規劃與開發中，將提供完整的數據分析、趨勢圖表、以及自訂報表功能。',
            'Analytics.PlannedFeatures': '規劃中的功能',
            'Analytics.Feature1': '風險指標趨勢分析',
            'Analytics.Feature2': '專案健康度儀表板',
            'Analytics.Feature3': '自訂報表生成與匯出',
            'Analytics.Feature4': '數據視覺化圖表',
            
            // Security
            'Security.Title': '安全設定',
            'Security.Subtitle': '安全設定與權限管理',
            'Security.ComingSoonTitle': '安全設定功能開發中',
            'Security.ComingSoonDesc': '此功能正在規劃與開發中，將提供完整的安全設定、權限管理、以及存取控制功能。',
            'Security.PlannedFeatures': '規劃中的功能',
            'Security.Feature1': '使用者認證與授權',
            'Security.Feature2': '角色與權限管理',
            'Security.Feature3': 'API 金鑰與存取控制',
            'Security.Feature4': '安全日誌與審計追蹤',
            
            // Settings
            'Settings.Title': '系統設定',
            'Settings.Subtitle': '系統設定與配置',
            'Settings.ComingSoonTitle': '系統設定功能開發中',
            'Settings.ComingSoonDesc': '此功能正在規劃與開發中，將提供完整的系統設定、配置管理、以及個人偏好設定功能。',
            'Settings.PlannedFeatures': '規劃中的功能',
            'Settings.Feature1': '系統基本設定',
            'Settings.Feature2': '通知與提醒設定',
            'Settings.Feature3': '個人偏好與主題設定',
            'Settings.Feature4': 'API 與整合設定',
            
            // Score Descriptions - M1
            'Score.M1.0': '極差：新人需要3個月以上才能上手',
            'Score.M1.5': '中等：新人需要1-2個月才能上手',
            'Score.M1.10': '極佳：新人幾乎不需要培訓就能上手',
            
            // Score Descriptions - M2
            'Score.M2.0': '極差：完全找不到設計決策的依據',
            'Score.M2.5': '中等：部分設計決策有依據，但不完整',
            'Score.M2.10': '極佳：設計決策有完整追溯鏈，可追到原始需求',
            
            // Score Descriptions - M3
            'Score.M3.0': '極差：完全無法預測變更影響',
            'Score.M3.5': '中等：能預測主要影響和大部分次要影響',
            'Score.M3.10': '極佳：能完整預測變更的所有影響，包括直接、間接和潛在影響',
            
            // Score Descriptions - M4
            'Score.M4.0': '極差：團隊對「完成」的理解完全不一致',
            'Score.M4.5': '中等：大部分理解一致，偶有歧義',
            'Score.M4.10': '極佳：團隊對「完成」有完整且統一的定義，無歧義',
            
            // Score Descriptions - M5
            'Score.M5.0': '極差：需要5次以上會議才能對齊',
            'Score.M5.5': '中等：通常需要2次會議才能對齊',
            'Score.M5.10': '極佳：完全不需要額外會議，溝通效率極高',
            
            // Score Table Labels
            'Score.Indicator': '指標',
            'Score.Value': '分數',
            'Score.Description': '說明',
            'Score.NoData': '尚未有評估數據',
            'Score.TableTitle': '各指標分數說明'
        },
        'en-US': {
            // Navigation
            'Nav.Dashboard': 'Dashboard',
            'Nav.Assessment': 'Assessment',
            'Nav.Report': 'Report',
            'Nav.Projects': 'Projects',
            'Nav.Documentation': 'Documentation',
            'Nav.Risks': 'Risks',
            
            // Common
            'Common.GenerateReport': 'Generate Report',
            'Common.Price': 'NT$2,990',
            'Common.Language': 'Language',
            'Common.Chinese': '繁體中文',
            'Common.English': 'English',
            
            // Dashboard
            'Dashboard.Title': 'Dashboard',
            'Dashboard.Subtitle': 'Project Overview and Risk Analysis',
            'Dashboard.Projects': 'Projects',
            'Dashboard.Completion': 'Document Completion',
            'Dashboard.RiskLevel': 'Risk Level',
            'Dashboard.RiskLow': 'Low',
            'Dashboard.RiskMedium': 'Medium',
            'Dashboard.RiskHigh': 'High',
            'Dashboard.RiskFingerprint': 'Risk Fingerprint',
            'Dashboard.RiskAnalysis': 'Risk Indicator Analysis',
            'Dashboard.TechRisk': 'Technical Risk',
            'Dashboard.ComplianceRisk': 'Compliance Risk',
            'Dashboard.OperationalRisk': 'Operational Risk',
            'Dashboard.FinancialRisk': 'Financial Risk',
            'Dashboard.MarketRisk': 'Market Risk',
            'Dashboard.HumanRisk': 'Human Risk',
            'Dashboard.FunctionModules': 'Function Modules',
            'Dashboard.QuickAccess': 'Quick Access to Main Features',
            'Dashboard.ProjectsTitle': 'Projects',
            'Dashboard.ProjectsDesc': 'Manage Projects and Documents',
            'Dashboard.AssessmentTitle': 'Assessment',
            'Dashboard.AssessmentDesc': 'System Risk Assessment Survey',
            'Dashboard.DocumentationTitle': 'Documentation',
            'Dashboard.DocumentationDesc': 'Document Management and Editing',
            'Dashboard.RisksTitle': 'Risks',
            'Dashboard.RisksDesc': 'Risk Assessment and Monitoring',
            'Dashboard.TeamTitle': 'Team',
            'Dashboard.TeamDesc': 'Team Collaboration Management',
            'Dashboard.AnalyticsTitle': 'Analytics',
            'Dashboard.AnalyticsDesc': 'Data Analysis Reports',
            'Dashboard.SecurityTitle': 'Security',
            'Dashboard.SecurityDesc': 'Security Settings and Permissions',
            'Dashboard.SettingsTitle': 'Settings',
            'Dashboard.SettingsDesc': 'System Settings and Configuration',
            
            // Assessment
            'Assessment.Title': 'Risk Assessment Survey',
            'Assessment.Subtitle': 'Please evaluate the following indicators based on your project status (0-10 points)',
            'Assessment.MaturityLabel': 'M = Maturity',
            'Assessment.MaturityDesc': ': Evaluate the maturity level of each process and capability. Higher scores indicate greater maturity in that area.',
            'Assessment.ProjectNameLabel': 'Project Name',
            'Assessment.ProjectNamePlaceholder': 'Enter project name',
            'Assessment.SystemNameLabel': 'System Name',
            'Assessment.SystemNamePlaceholder': 'Enter the system name to assess',
            'Assessment.SystemCodeLabel': 'System Code',
            'Assessment.SystemCodePlaceholder': 'Enter system code (e.g., ADM)',
            'Assessment.EvaluatorRoleLabel': 'Evaluator Role',
            'Assessment.EvaluatorRolePlaceholder': 'Enter or select evaluator role (e.g., PM, SA, PG)',
            'Assessment.M1': 'M1 System Handover',
            'Assessment.M1Desc': 'Can new team members get up to speed within 1 month? (Higher score = faster onboarding)',
            'Assessment.M2': 'M2 Requirements Traceability',
            'Assessment.M2Desc': 'Can design decisions be traced to their basis?',
            'Assessment.M3': 'M3 Change Prediction',
            'Assessment.M3Desc': 'Can impact be predicted before changing requirements?',
            'Assessment.M4': 'M4 Acceptance Criteria',
            'Assessment.M4Desc': 'Does the team have a consistent understanding of "done"?',
            'Assessment.M5': 'M5 Communication Cost',
            'Assessment.M5Desc': 'How efficient is cross-role alignment? (Higher score = more efficient, fewer meetings needed)',
            'Assessment.OpenQ6': 'Question 6. Please describe the main challenges your project faces',
            'Assessment.OpenQ7': 'Question 7. Please specify areas you would like to improve',
            'Assessment.OpenQ8': 'Question 8. Please provide other relevant information',
            'Assessment.PlaceholderQ6': 'e.g., Frequent requirement changes, incomplete documentation, team communication difficulties, technical debt accumulation...',
            'Assessment.PlaceholderQ7': 'e.g., Establish better requirements traceability, improve documentation quality, optimize change management, enhance team collaboration...',
            'Assessment.PlaceholderQ8': 'e.g., Project scale, team size, development cycle, technology stack, special business requirements...',
            
            // Report
            'Report.Title': 'Risk Assessment Report',
            'Report.Subtitle': 'Risk fingerprint analysis automatically generated based on your questionnaire results',
            'Report.RadarTitle': 'M1-M5 Score Radar Chart',
            'Report.RadarDesc': 'Visualizes five key risk indicators',
            'Report.RadarM1': 'M1 Handover',
            'Report.RadarM2': 'M2 Traceability',
            'Report.RadarM3': 'M3 Change',
            'Report.RadarM4': 'M4 Acceptance',
            'Report.RadarM5': 'M5 Communication',
            'Report.FingerprintTitle': 'Risk Fingerprint Summary',
            'Report.FingerprintDesc': 'Overall Risk Profile and Recommendations',
            'Report.ProjectName': 'Project Name',
            'Report.SystemName': 'System Name',
            'Report.SystemCode': 'System Code',
            'Report.EvaluatorRole': 'Evaluator Role',
            'Report.ReportId': 'REPORT ID',
            'Report.AssessmentTime': 'Assessment Time',
            'Report.RiskLevelThreshold': '0–20 High, 21–35 Medium, 36–50 Low',
            'Report.TotalScore': 'Total Score',
            'Report.RiskLevel': 'Risk Level',
            'Report.SampleTime': 'Sample Time',
            'Report.ImprovementAdvice': 'Risk Improvement Recommendations',
            'Report.DefaultAdvice': 'Specific recommendations will be generated based on your survey results.',
            'Report.DeliverablesTitle': 'Recommended Minimum Deliverables for This Assessment:',
            'Report.NoData': 'No survey data found. Please complete the assessment first.',
            'Report.NoReportTitle': 'No Report Generated',
            'Report.NoReportDesc': 'Complete the risk assessment questionnaire and make payment to view your risk assessment report.',
            'Report.GoToAssessment': 'Go to Assessment Questionnaire',
            'Report.AdviceLow': 'M3 Change Prediction is low. Recommend strengthening requirements traceability and impact analysis processes to better link design decisions with requirement sources.',
            'Report.AdviceNormal': 'Overall change prediction capability is acceptable. Recommend continuing to optimize requirements traceability and acceptance criteria to further reduce communication costs and risks.',
            'Report.OpenQuestions': 'Additional Notes',
            'Report.OpenQuestionsDesc': 'Your detailed notes and additional information',
            'Report.NoOpenAnswers': 'No additional notes provided',
            'Report.DownloadPDF': 'Download PDF Report',
            'Report.ViewTracking': 'View 3-Month Tracking',
            'Report.InviteColleague': 'Invite Colleague NT$2,490/person (17% discount)',
            'Report.AIInsights': 'Risk Insights',
            'Report.AIInsightsDesc': 'In-depth analysis based on M6, M7, M8 additional notes',
            'Report.Analyzing': 'Analyzing...',
            'Report.TipLabel': 'Tip',
            'Report.RefreshHint': 'You can refresh the page (F5)',
            'Report.AIRefreshHint': 'to generate alternative suggestions (for brainstorming)',
            'Report.ActionPlanTitle': '30-Day Action Plan',
            'Report.ActionPlanDesc': 'Next Steps Action Recommendations for This Assessment',
            'Report.DisclaimerContent': 'This report is generated based on user-provided data.\n\nThis report is applicable for:\n‧ Software project risk assessment\n‧ System handover and governance review\nDoes not replace:\n‧ Audit\n‧ Regulatory certification\n‧ Code security scanning',
            
            // Projects
            'Projects.Title': 'Project Management',
            'Projects.Subtitle': 'Manage your projects and documents',
            'Projects.ComingSoonTitle': 'Project Management Feature Under Development',
            'Projects.ComingSoonDesc': 'This feature is currently being planned and developed. It will provide comprehensive project management, document version control, and team collaboration capabilities.',
            'Projects.PlannedFeatures': 'Planned Features',
            'Projects.Feature1': 'Project creation and management',
            'Projects.Feature2': 'Document version control and history',
            'Projects.Feature3': 'Team collaboration and permission management',
            'Projects.Feature4': 'Automated document generation and updates',
            
            // Documentation
            'Documentation.Title': 'Documentation Management',
            'Documentation.Subtitle': 'Auto-generation and version control',
            'Documentation.ComingSoonTitle': 'Documentation Management Feature Under Development',
            'Documentation.ComingSoonDesc': 'This feature is currently being planned and developed. It will provide AI-driven document auto-generation, version control, and document template management capabilities.',
            'Documentation.PlannedFeatures': 'Planned Features',
            'Documentation.Feature1': 'AI-powered technical document generation',
            'Documentation.Feature2': 'Document version control and history',
            'Documentation.Feature3': 'Document template library and custom templates',
            'Documentation.Feature4': 'Code and database analysis integration',
            
            // Risks
            'Risks.Title': 'Risk Management',
            'Risks.Subtitle': 'Risk monitoring and analysis',
            'Risks.ComingSoonTitle': 'Risk Management Feature Under Development',
            'Risks.ComingSoonDesc': 'This feature is currently being planned and developed. It will provide comprehensive risk monitoring, trend analysis, and risk alert capabilities.',
            'Risks.PlannedFeatures': 'Planned Features',
            'Risks.Feature1': 'Real-time risk indicator monitoring',
            'Risks.Feature2': 'Risk trend analysis and prediction',
            'Risks.Feature3': 'Risk alerts and notification mechanisms',
            'Risks.Feature4': 'Risk improvement tracking and reporting',
            
            // Team
            'Team.Title': 'Team Management',
            'Team.Subtitle': 'Team collaboration and permission management',
            'Team.ComingSoonTitle': 'Team Management Feature Under Development',
            'Team.ComingSoonDesc': 'This feature is currently being planned and developed. It will provide comprehensive team collaboration, member management, and permission control capabilities.',
            'Team.PlannedFeatures': 'Planned Features',
            'Team.Feature1': 'Team member management and invitations',
            'Team.Feature2': 'Role and permission management',
            'Team.Feature3': 'Collaborative workspace and project assignment',
            'Team.Feature4': 'Team activity logs and notifications',
            
            // Analytics
            'Analytics.Title': 'Data Analytics',
            'Analytics.Subtitle': 'Data analysis and reporting',
            'Analytics.ComingSoonTitle': 'Data Analytics Feature Under Development',
            'Analytics.ComingSoonDesc': 'This feature is currently being planned and developed. It will provide comprehensive data analysis, trend charts, and custom reporting capabilities.',
            'Analytics.PlannedFeatures': 'Planned Features',
            'Analytics.Feature1': 'Risk indicator trend analysis',
            'Analytics.Feature2': 'Project health dashboard',
            'Analytics.Feature3': 'Custom report generation and export',
            'Analytics.Feature4': 'Data visualization charts',
            
            // Security
            'Security.Title': 'Security Settings',
            'Security.Subtitle': 'Security settings and permission management',
            'Security.ComingSoonTitle': 'Security Settings Feature Under Development',
            'Security.ComingSoonDesc': 'This feature is currently being planned and developed. It will provide comprehensive security settings, permission management, and access control capabilities.',
            'Security.PlannedFeatures': 'Planned Features',
            'Security.Feature1': 'User authentication and authorization',
            'Security.Feature2': 'Role and permission management',
            'Security.Feature3': 'API keys and access control',
            'Security.Feature4': 'Security logs and audit trails',
            
            // Settings
            'Settings.Title': 'System Settings',
            'Settings.Subtitle': 'System settings and configuration',
            'Settings.ComingSoonTitle': 'System Settings Feature Under Development',
            'Settings.ComingSoonDesc': 'This feature is currently being planned and developed. It will provide comprehensive system settings, configuration management, and personal preference settings.',
            'Settings.PlannedFeatures': 'Planned Features',
            'Settings.Feature1': 'Basic system settings',
            'Settings.Feature2': 'Notification and reminder settings',
            'Settings.Feature3': 'Personal preferences and theme settings',
            'Settings.Feature4': 'API and integration settings',
            
            // Score Descriptions - M1
            'Score.M1.0': 'Very Poor: New team members need more than 3 months to get up to speed',
            'Score.M1.5': 'Medium: New team members need 1-2 months to get up to speed',
            'Score.M1.10': 'Excellent: New team members can get up to speed with almost no training',
            
            // Score Descriptions - M2
            'Score.M2.0': 'Very Poor: Cannot find any basis for design decisions',
            'Score.M2.5': 'Medium: Some design decisions have basis, but incomplete',
            'Score.M2.10': 'Excellent: Design decisions have complete traceability chain to original requirements',
            
            // Score Descriptions - M3
            'Score.M3.0': 'Very Poor: Cannot predict change impact at all',
            'Score.M3.5': 'Medium: Can predict main impacts and most secondary impacts',
            'Score.M3.10': 'Excellent: Can fully predict all impacts of changes including direct, indirect, and potential impacts',
            
            // Score Descriptions - M4
            'Score.M4.0': 'Very Poor: Team has completely inconsistent understanding of "done"',
            'Score.M4.5': 'Medium: Mostly consistent understanding, occasional ambiguity',
            'Score.M4.10': 'Excellent: Team has complete and unified definition of "done" with no ambiguity',
            
            // Score Descriptions - M5
            'Score.M5.0': 'Very Poor: Need more than 5 meetings to align',
            'Score.M5.5': 'Medium: Usually need 2 meetings to align',
            'Score.M5.10': 'Excellent: No additional meetings needed, extremely efficient communication',
            
            // Score Table Labels
            'Score.Indicator': 'Indicator',
            'Score.Value': 'Score',
            'Score.Description': 'Description',
            'Score.NoData': 'No assessment data available',
            'Score.TableTitle': 'Score Description for Each Indicator'
        }
    },
    
    // 根據分數獲取對應的說明（根據分數區間返回對應說明）
    getScoreDescription(metric, score) {
        const key = this.currentLang;
        const translations = this.translations[key];
        if (!translations) return '';
        
        // 將分數轉換為數字
        const numScore = Number(score);
        if (isNaN(numScore) || numScore < 0 || numScore > 10) return '';
        
        // 根據分數區間選擇對應的說明
        let scoreKey;
        if (numScore <= 2.5) {
            scoreKey = `Score.${metric}.0`; // 極差
        } else if (numScore <= 7.5) {
            scoreKey = `Score.${metric}.5`; // 中等
        } else {
            scoreKey = `Score.${metric}.10`; // 極佳
        }
        
        return translations[scoreKey] || '';
    },
    
    t(key) {
        return this.translations[this.currentLang]?.[key] || key;
    },
    
    setLang(lang) {
        console.log('[i18n] 設置語言:', lang);
        this.currentLang = lang;
        // 使用 Cookie 保存語言偏好（HttpOnly 由後端設置，這裡設置客戶端 Cookie 作為備份）
        setCookie('lang', lang, 365);
        console.log('[i18n] Cookie 已設置，驗證:', getCookie('lang'));
        // 同時通知後端保存到 HttpOnly Cookie
        fetch('/Home/SetLanguage', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ lang: lang }),
            credentials: 'same-origin' // 確保發送 Cookie
        }).then(() => {
            console.log('[i18n] 後端 Cookie 已設置');
        }).catch(err => console.warn('[i18n] 設置語言 Cookie 失敗:', err));
        
        this.updatePage();
        // 觸發自定義事件，讓其他腳本知道語言已變更
        window.dispatchEvent(new CustomEvent('languageChanged', { detail: { lang } }));
    },
    
    updatePage() {
        // 更新所有帶有 data-i18n 屬性的元素（包括 SVG text）
        document.querySelectorAll('[data-i18n]').forEach(el => {
            const key = el.getAttribute('data-i18n');
            // SVG text 元素使用 textContent，其他元素也使用 textContent
            el.textContent = this.t(key);
        });
        
        // 更新 placeholder
        document.querySelectorAll('[data-placeholder-i18n]').forEach(el => {
            const key = el.getAttribute('data-placeholder-i18n');
            el.placeholder = this.t(key);
        });
        
        // 更新語言按鈕狀態（包括 .lang-btn 和 .lang-btn-top）
        document.querySelectorAll('.lang-btn, .lang-btn-top').forEach(btn => {
            const btnLang = btn.getAttribute('data-lang');
            if (btnLang === this.currentLang) {
                btn.classList.add('active');
                if (btn.classList.contains('lang-btn-top')) {
                    btn.style.background = 'rgba(98, 0, 234, 0.3)';
                    btn.style.borderColor = 'rgba(98, 0, 234, 0.5)';
                    btn.style.color = '#ffffff';
                } else {
                    btn.style.background = 'rgba(98, 0, 234, 0.3)';
                    btn.style.borderColor = 'rgba(98, 0, 234, 0.5)';
                    btn.style.color = '#ffffff';
                }
            } else {
                btn.classList.remove('active');
                if (btn.classList.contains('lang-btn-top')) {
                    btn.style.background = 'transparent';
                    btn.style.borderColor = 'transparent';
                    btn.style.color = 'var(--sidebar-text)';
                } else {
                    btn.style.background = 'rgba(255, 255, 255, 0.05)';
                    btn.style.borderColor = 'rgba(255, 255, 255, 0.1)';
                    btn.style.color = 'var(--sidebar-text)';
                }
            }
        });
        
        // 更新 URL 參數
        const url = new URL(window.location);
        url.searchParams.set('culture', this.currentLang);
        window.history.replaceState({}, '', url);
    },
    
    init() {
        // 優先從 Cookie 讀取語言設定（用戶最近選擇的語言）
        const cookieLang = getCookie('lang');
        console.log('[i18n] 初始化：從 Cookie 讀取語言:', cookieLang);
        
        if (cookieLang && (cookieLang === 'zh-TW' || cookieLang === 'en-US')) {
            this.currentLang = cookieLang;
            console.log('[i18n] 使用 Cookie 中的語言:', this.currentLang);
        } else {
            // 如果 Cookie 中沒有語言設定，使用預設值
            this.currentLang = this.currentLang || 'zh-TW';
            console.log('[i18n] 使用預設語言:', this.currentLang);
        }
        
        // 如果 URL 參數中有 culture，且與 Cookie 不同，則使用 URL 參數（但這通常不會發生）
        const urlParams = new URLSearchParams(window.location.search);
        const langParam = urlParams.get('culture');
        if (langParam && (langParam === 'zh-TW' || langParam === 'en-US') && langParam !== this.currentLang) {
            console.log('[i18n] URL 參數中的語言與 Cookie 不同，使用 URL 參數:', langParam);
            this.setLang(langParam);
        } else {
            // 更新頁面顯示，並同步 URL 參數
            console.log('[i18n] 更新頁面顯示，當前語言:', this.currentLang);
            this.updatePage();
        }
    }
};

// 頁面載入時初始化
document.addEventListener('DOMContentLoaded', () => {
    i18n.init();
});

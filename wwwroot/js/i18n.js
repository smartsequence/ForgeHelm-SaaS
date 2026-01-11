// 簡化的多語言支援系統
const i18n = {
    currentLang: localStorage.getItem('lang') || 'zh-TW',
    
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
            'Assessment.SystemNameLabel': '系統名稱',
            'Assessment.SystemNamePlaceholder': '請輸入要評估的系統名稱',
            'Assessment.M1': 'M1 系統交接',
            'Assessment.M1Desc': '新人不需1個月上手？',
            'Assessment.M2': 'M2 需求追溯',
            'Assessment.M2Desc': '設計決策能找到依據？',
            'Assessment.M3': 'M3 變更預測',
            'Assessment.M3Desc': '改需求前能預知影響？',
            'Assessment.M4': 'M4 驗收標準',
            'Assessment.M4Desc': '團隊對「完成」理解一致？',
            'Assessment.M5': 'M5 溝通成本',
            'Assessment.M5Desc': '跨角色對齊需幾次會議？',
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
            'Report.SystemName': '系統名稱',
            'Report.AssessmentDate': '評估日期',
            'Report.TotalScore': '總分',
            'Report.RiskLevel': '風險等級',
            'Report.SampleTime': '樣本時間',
            'Report.ImprovementAdvice': '風險改善建議',
            'Report.DefaultAdvice': '將根據您的問卷結果產生具體建議。',
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
            'Report.AIRefreshHint': '💡 提示：可以重新整理頁面（F5）獲取不同版本的 AI 建議',
            'Report.DisclaimerTitle': '本報告適用於',
            'Report.DisclaimerContent': '本報告適用於：\n‧ 軟體專案風險盤點\n‧ 系統交接與治理檢視\n不取代：\n‧ 稽核\n‧ 法規認證\n‧ 程式碼安全掃描',
            
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
            'Score.M3.5': '中等：能預測部分影響，但可能遺漏',
            'Score.M3.10': '極佳：能完整預測變更的所有影響，包括間接影響',
            
            // Score Descriptions - M4
            'Score.M4.0': '極差：團隊對「完成」的理解完全不一致',
            'Score.M4.5': '中等：大部分理解一致，偶有歧義',
            'Score.M4.10': '極佳：團隊對「完成」有完整且統一的定義，無歧義',
            
            // Score Descriptions - M5
            'Score.M5.0': '極差：需要5次以上會議才能對齊',
            'Score.M5.5': '中等：需要2-3次會議才能對齊',
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
            'Assessment.SystemNameLabel': 'System Name',
            'Assessment.SystemNamePlaceholder': 'Enter the system name to assess',
            'Assessment.M1': 'M1 System Handover',
            'Assessment.M1Desc': 'Can new team members get up to speed within 1 month?',
            'Assessment.M2': 'M2 Requirements Traceability',
            'Assessment.M2Desc': 'Can design decisions be traced to their basis?',
            'Assessment.M3': 'M3 Change Prediction',
            'Assessment.M3Desc': 'Can impact be predicted before changing requirements?',
            'Assessment.M4': 'M4 Acceptance Criteria',
            'Assessment.M4Desc': 'Does the team have a consistent understanding of "done"?',
            'Assessment.M5': 'M5 Communication Cost',
            'Assessment.M5Desc': 'How many meetings are needed to align across roles?',
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
            'Report.SystemName': 'System Name',
            'Report.AssessmentDate': 'Assessment Date',
            'Report.TotalScore': 'Total Score',
            'Report.RiskLevel': 'Risk Level',
            'Report.SampleTime': 'Sample Time',
            'Report.ImprovementAdvice': 'Risk Improvement Recommendations',
            'Report.DefaultAdvice': 'Specific recommendations will be generated based on your survey results.',
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
            'Report.AIRefreshHint': '💡 Tip: You can refresh the page (F5) to get different versions of AI suggestions',
            'Report.DisclaimerTitle': 'This report is applicable for:',
            'Report.DisclaimerContent': 'This report is applicable for:\n‧ Software project risk assessment\n‧ System handover and governance review\nDoes not replace:\n‧ Audit\n‧ Regulatory certification\n‧ Code security scanning',
            
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
            'Score.M3.5': 'Medium: Can predict some impacts, but may miss others',
            'Score.M3.10': 'Excellent: Can fully predict all impacts of changes, including indirect impacts',
            
            // Score Descriptions - M4
            'Score.M4.0': 'Very Poor: Team has completely inconsistent understanding of "done"',
            'Score.M4.5': 'Medium: Mostly consistent understanding, occasional ambiguity',
            'Score.M4.10': 'Excellent: Team has complete and unified definition of "done" with no ambiguity',
            
            // Score Descriptions - M5
            'Score.M5.0': 'Very Poor: Need more than 5 meetings to align',
            'Score.M5.5': 'Medium: Need 2-3 meetings to align',
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
        this.currentLang = lang;
        localStorage.setItem('lang', lang);
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
        // 從 URL 參數讀取語言
        const urlParams = new URLSearchParams(window.location.search);
        const langParam = urlParams.get('culture');
        if (langParam && (langParam === 'zh-TW' || langParam === 'en-US')) {
            this.setLang(langParam);
        } else {
            this.updatePage();
        }
    }
};

// 頁面載入時初始化
document.addEventListener('DOMContentLoaded', () => {
    i18n.init();
});

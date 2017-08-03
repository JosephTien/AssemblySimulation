# Thin structure fabrication
* **注意事項**
    * 此為Unity Project。
    * 由於程式原先為個人開發，所以為圖方便，並未維持易讀性及整體架構的正規化lol...
        * 之後會慢慢地修改，若有測試或修改的需求可先通知作者協調。
        
* **Input**
    * 根據輸入的數字讀取`inputSet/`資料夾中對應編號的testcase
    * 標號後面加上乘號和數字可以scale，如`1031*1000`
    
* **Lin (Topo editor)**
    * **Load:** 讀取`thinstruct.lin` *(格式說明於附錄)*
    * **Re:** 讀取`thinstruct.txt` *(格式說明於附錄)*
    * **SPR:** 讀取`thinstruct.txt` *(格式說明於附錄)*
    * **Simp:** 簡化由點Q、點W、點E所定義的section
        * 分別按住QWE並用滑鼠點擊即可手動選取該三點
    * **Next:** 自動選取下一個section
    * **Auto:** 自動簡化所有的section
    * **Merge:** 將點R1、點R2合併為一個點，並刪除中間的edge
        * 按住R並用滑鼠點擊不同的node即可選出R1及R2
    * **Add:** 在點R1、點R2間生成新的一個edge
    * **Del:** 刪除點R1、點R2間的edge
    * **Save/Write** 將結果儲存於`thinstruct.txt`
    * **Show Junc:** 顯示junction node (branch>=3)
    * **Show Impo:** 顯示關鍵node (junction node, end node, sharp angle node)
    * **Show edge:** 顯示edge
    * **Hide Node:** 隱藏node
    * **Cross:** 將距離小於threshold的node pair合併成一個node
        * 手動輸入threshold在上方的欄位。
        * 該功能用以修復隱含問題的input topo，
        * 同時找出關鍵node。
        * 由這些關鍵node可以自動找出所有的section。
        
* **Edit (Merge Component Editor)**
    * **Load:** 讀取`thinstruct.txt`
    * **Group** 自動找出curve並分析。
    * **Show Sol:** 顯示各個component是否有解
    * **Show Impo:** 顯示各個component所隸屬的group
    * **Show edge:** 顯示切面
    * **Hide Node:** 顯示被選擇的curve在指定的拆除方向下，有哪些component會被collide
        * 壓住空白鍵並使用滑鼠點擊可以選取
        * 選取後壓住空白鍵並使用滑鼠滾輪可改變切面
        * 使用YUIOHJKL可以調整移除方向 (實作中、暫時不詳細說明)
    * **Save/Gsave:** 儲存所有編輯資訊以便後續生成
    
* **Generate (CSG output generator)**
    * 根據前面的編輯資訊，利用CSG製作成品
    * 計算完成之後，需要分別依序手動執行`csgcommandlinetool/^.bat`以及`csgcommandlinetool/^.bat`
    
* **Simulate**
    * 按下Load讀取output以及拆除方向的資訊
    * 按下Split開始模擬結果
    * 按下View可以切換不同預設視角

* **其他說明**
    * 跟一般的檢視器一樣可以滑鼠拖曳以及滾輪改變視角
    * 必須要根據Edit、Generate的順序生成data才能Simulate

* **附錄**
    * topo格式 *(to be continue)*
    * 相關資訊的紀錄方式 *(to be continue)*
    * cache格式 *(to be continue)*
    
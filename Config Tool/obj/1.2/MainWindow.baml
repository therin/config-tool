   M S B A M L   `   `   `  ����   Config Tool    Config_Tool.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006       � Title$  SmartTrade Config Tool�� � IsSharedSizeScope . $	��100��$	��359��$
��Left=�$	��Top=�$��847.5��$��451.5�� � 
ResizeMode$ NoResize=�.���  � ColumnDefinitions �� .+���  J� RenderTransformOrigin$ 0.5,0.5&�z�V� .~��� ��  �� AngleX$ 0.128Q��� T�  T� X$ 0.998Q�.@��� $��Configuration��$��0q�.��� ���	#FFE5E5E5 � RowDefinitions �� $��2* ���  �� $	��66* ��� $	��73* �.+��� $��Client Config��$
��Left=�$��	10,10,0,0q�$	��Top=�$	��125��$	��382��.���  �� $
��147* ��� $
��223* �.+��� -    ��Name$ clientTextbox��$
��Left=�$��26��$��3,53,0,0q�	 �� TextWrapping$
	 Wrap=�$	��Top=�$	��139��
 �� TextAlignment$
 Center=�$��Center=�$��Center=��� -    ��Name$ buttonClient��$��Update Client Config��$
��Left=�$��	85,53,0,0q�$	��Top=�$	��128��$ 	3.827,2.5&�$��26��$��1���� -    ��Name$ currentClient��$�� ��$
��Left=�$��
131,12,0,0q�$	��Top=�$ -0.821,1.154&�$��26��$	��153��$��2���� $��Current Value:��$
��Left=�$��	10,12,0,0q�$	��Top=�$	��100���� -   $ clientPortTextbox��$��26��$��
0,53,143,0q�$
	 Wrap=�$	��Top=�$��1��$��Center=�$��Center=��� -   $ currentClient_Copy��$�� ��$
��Left=�$��
153,12,0,0q�$	��Top=�$ -0.821,1.154&�$��26��$��60��$��1���� $��Mobile Config��$
��Left=�$��3,10,0,0q�$	��Top=�$	��125��$	��423��$��1��.���  �� $	��86* ��� $	��61* �.+��� -   $ mobileTextbox��$
��Left=�$��26��$��	10,53,0,0q�$
	 Wrap=�$	��Top=�$	��159��$
 Center=�$��Center=�$��Center=��� -   $ buttonMobile��$��Update Mobile Config��$
��Left=�$��	21,53,0,0q�$	��Top=�$	��128��$ 	3.827,2.5&�$��26��$��1���� -   $ currentMobile��$�� ��$
��Left=�$��
131,12,0,0q�$	��Top=�$ -0.821,1.154&�$��26��$	��153��$��2���� $��Current Value:��$
��Left=�$��	10,12,0,0q�$	��Top=�$	��100���� -	   $ mobilePortTextbox��$
��Left=�$��27��$��
174,52,0,0q�$
	 Wrap=�$	��Top=�$��82��$��Center=�$��Center=�$��2���� -
   $ currentMobile_Copy��$�� ��$
��Left=�$��	65,12,0,0q�$	��Top=�$ -0.821,1.154&�$��26��$��84��$��1���� $��SmartTime Config��$
��Left=�$��	1,140,0,0q�$	��Top=�$	��118��$	��425��$��1��.���  �� $
��155* ��� $
��215* ��� $	��55* ��� $��7* �.+��� -   $ timeTextbox��$
��Left=�$��26��$��	10,53,0,0q�$
	 Wrap=�$	��Top=�$	��172��$
 Center=�$��Center=�$��Center=�$��2���� -   $ 
buttonTime��$��Update ST Config��$
��Left=�$��
128,53,0,0q�$	��Top=�$	��128��$ 	3.827,2.5&�$��26��$��2��$��1���� -   $ currentTime��$�� ��$��	10,12,0,0q�$	��Top=�$ -0.821,1.154&�$��26��$��1��$
��Left=�$	��153���� $��Current Value:��$
��Left=�$��	10,12,0,0q�$	��Top=�$	��100���� -   $ timePortTextbox��$
��Left=�$��27��$��	39,52,0,0q�$
	 Wrap=�$	��Top=�$��84��$��1��$��Center=�$��Center=��� -   $ currentTime_Copy��$�� ��$
��Left=�$��
198,12,0,0q�$	��Top=�$ -0.821,1.154&�$��26��$��49��$��1��$��2���� $��SmartGate Config��$
��Left=�$��
10,140,0,0q�$	��Top=�$	��118��$	��382��.���  �� .+��� -   $ gateTextbox��$
��Left=�$��26��$��6,53,0,0q�$
	 Wrap=�$	��Top=�$	��136��$
 Center=�$��Center=�$��Center=��� -   $ 
buttonGate��$��Update SG Config��$
��Left=�$��
232,53,0,0q�$	��Top=�$	��128��$ 	3.827,2.5&�$��26���� -   $ currentGate��$�� ��$
��Left=�$��
152,12,0,0q�$	��Top=�$ -0.821,1.154&�$��26��$	��153���� $��Current Value:��$
��Left=�$��	10,12,0,0q�$	��Top=�$	��100���� -   $ gatePortTextbox��$
��Left=�$��27��$��
147,53,0,0q�$
	 Wrap=�$	��Top=�$��80��$��Center=�$��Center=��� -   $ currentGate_Copy��$�� ��$
��Left=�$��
305,12,0,0q�$	��Top=�$ -0.821,1.154&�$��26��$��55���� $��Service Config��$
��Left=�$��10,1,0,0q�$	��Top=�$	��118��$	��812��$��2��$��1��.���  �� .+��� -   $ buttonService��$��Update Service Config��$
��Left=�$��
651,43,0,0q�$	��Top=�$	��144��$ 	3.827,2.5&�$��39���� -   $ currentService��$�� ��$
��Left=�$��
100,10,0,0q�$	��Top=�$ -0.821,1.154&�$��26��$	��520���� $��Current Value:��$
��Left=�$��	10,10,0,0q�$	��Top=�$	��100���� -   $ serviceDBTextbox��$
��Left=�$��40��$��	10,43,0,0q�$
	 Wrap=�$	��Top=�$	��539��$
 Center=�$ 	1.149,0.5&�$��Center=�$��Center=��� -   $ servicePortTextbox��$��38��$��
554,44,0,0q�$
	 Wrap=�$	��Top=�$��92��$
��Left=�$��Center=�$��Center=�$ 0.321,0.553&��� -   $ currentService_Copy��$�� ��$
��Left=�$��
651,10,0,0q�$	��Top=�$ -0.821,1.154&�$��26��$	��144���� $��Services��$
��Left=�$��20��$	��Top=�$��71��$��-1,0,0,0q�.��� ���	#FFE5E5E5$��0,3,0,-3q�.+��� $��Create Services��.���  �� $
��201* ��� $
��334* ��� $
��187* ��� $
��100* � �� $
��246* ��� $
��125* �.+��� $��SmartTrade Service Name��$
��Left=�$��	30,10,0,0q�$	��Top=�$ 	-2.474,-1&�$	��146���� $��Mobile Service Name��$
��Left=�$��	30,85,0,0q�$	��Top=�$ 	-2.474,-1&�$	��144���� $��SmartTime Service Name��$
��Left=�$��
27,165,0,0q�$	��Top=�$ 	-2.474,-1&�$	��147���� -   $ STServiceTextbox��$
��Left=�$��38��$��	10,41,0,0q�$
	 Wrap=�$	��Top=�$	��187��$��SmartTrade V6 Service��$��Center=�$��Center=��� -   $ STMobileTextbox��$
��Left=�$��41��$��
10,116,0,0q�$
	 Wrap=�$	��Top=�$	��187��$��SmartTrade Mobile Service��$��Center=�$��Center=��� -   $ STTimeTextbox��$
��Left=�$��39��$��
10,196,0,0q�$
	 Wrap=�$	��Top=�$	��187��$"��SmartTrade SmartTime Service��$��Center=�$��Center=��� -   $��Create��$
��Left=�$��3.604,40,0,0q�$	��Top=�$��86��$��39��$ 0.782,0.436&�$��3���� -   $ createMobileButton��$��Create��$
��Left=�$��3.604,119,0,0q�$	��Top=�$��86��$��38��$ 0.896,0.474&�$��3���� -   $ createTimeButton��$��Create��$
��Left=�$��3.604,196,0,0q�$	��Top=�$��86��$ 	0.933,0.5&�$��39��$��3���� -    $ servicePathTextbox��$
��Left=�$��38��$��4,42,0,0q�$
	 Wrap=�$	��Top=�$	��321��$ 0.504,0.167&�$��Center=�$��Center=�$��1���� -!   $ mobilePathTextbox��$
��Left=�$��42��$��	4,115,0,0q�$
	 Wrap=�$	��Top=�$	��321��$��Center=�$��Center=�$��1���� -"   $ timeTextbox_Copy��$
��Left=�$��39��$��	4,196,0,0q�$
	 Wrap=�$	��Top=�$	��321��$��Center=�$��Center=�$��1���� -#    ��Name$ ServiceComboBox��$
��Left=�$��3,42,0,0q�$	��Top=�$	��175��$��38��$��Center=�$��Center=�$��2���� -$   $ mobileSComboBox��$
��Left=�$��	3,116,0,0q�$	��Top=�$	��175��$��41��$��Center=�$��Center=�$��2���� -%   $ smarttimeSCombobox��$
��Left=�$��	3,196,0,0q�$	��Top=�$	��175��$��39��$��Center=�$��Center=�$��2���� $��SmartTrade Service Path��$
��Left=�$��	98,10,0,0q�$	��Top=�$ 	-2.474,-1&�$	��146��$��1���� $#��SmartTrade Service Dependency��$
��Left=�$��3,10,0,0q�$	��Top=�$ 	-2.474,-1&�$	��183��$��2���� $��Mobile Service Path��$
��Left=�$��	98,85,0,0q�$	��Top=�$ 	-2.474,-1&�$	��146��$��1���� $��Mobile Service Dependency��$
��Left=�$��	13,85,0,0q�$	��Top=�$ 	-2.474,-1&�$	��183��$��2��$��2���� $"��SmartTime Service Dependency��$
��Left=�$��	3,162,0,0q�$	��Top=�$ 	-2.474,-1&�$	��183��$��2���� $��SmartTime Service Path��$
��Left=�$��
98,165,0,0q�$	��Top=�$ 	-2.474,-1&�$	��146��$��1���� -&   $ 
logTextbox��$
��Left=�$��95��$��	98,20,0,0q�$
	 Wrap=�$	��Top=�$	��513��$��1��$��1��$��3���� -'   $ deleteSButton��$��Delete��$
��Left=�$��	10,82,0,0q�$	��Top=�$��75��$ 0.387,-1.45&�$��1��$��1���� -(   $ startSButton��$��Start��$
��Left=�$��	10,32,0,0q�$	��Top=�$��75��$ 
0.493,-0.4&�$��1��$��1���� -)   $ stopSButton��$
��Stop��$
��Left=�$��	10,57,0,0q�$	��Top=�$��75��$ 
0.529,0.25&�$��1��$��1���� $ ��List of relevant services:��$
��Left=�$��
10,236,0,0q�$	��Top=�$ -0.432,0.256&�$��2���� -*    ��Name$ servicesListbox��$
��Left=�$��95��$��	10,20,0,0q�$	��Top=�$	��190��$ -0.844,2.008&�$��1���� $��Database��$
��Left=�$��20��$	��Top=�$��77��$��-1,0,0,0q�$ 0.502,0.167&�.��� ���	#FFE5E5E5 �� $
��233* ��� $
��160* � �� $
��129* ��� $
��149* �.+��� -+   $ logTextbox2��$
��Left=�$��88��$��
10,136,0,0q�$
	 Wrap=�$	��Top=�$	��373��$��Center=�$��Center=��� $��	Downloads��$
��Left=�$��	10,10,0,0q�$	��Top=�$	��121��$	��373��.���  �� $	��40* ��� $	��25* ��� $	��34* � �� $
��114* ��� $
��156* ��� $	��91* �.+��� -,   $ downloadCombobox��$
��Left=�$��4,39,0,0q�$	��Top=�$	��355��$��24�� �� 
IsEditable .  �� SelectedIndex$ 0��$��3��$��2��.@��� --    ��Name$
 cbi1��$;��5http://smarttrade.biz/downloads/SQLEXPRWT_x64_ENU.exe���� -.   $
 cbi2��$;��5http://smarttrade.biz/downloads/SQLEXPRWT_x86_ENU.exe���� -/   $
 cbi3��$8��2http://smarttrade.biz/downloads/db/v6blank-AUS.bak���� -0   $
 cbi4��$7��1http://smarttrade.biz/downloads/db/v6blank-NZ.bak���� -1   $
 cbi5��$7��1http://smarttrade.biz/downloads/db/v6demo-AUS.bak���� -2   $
 cbi6��$6��0http://smarttrade.biz/downloads/db/v6demo-NZ.bak���� -3   $ sqlDownloadButton��$��Download��$
��Left=�$��4,3,0,0q�$	��Top=�$	��106��$��28��$ 0.323,-0.217&�$��2��� -4    �Name$ progressBar��$
��Left=�$��28��$��1,3,0,0q�$	��Top=�$	��153��$ -2.33,-2&�$��1��$��2���� -5   $ 
labelSpeed��$�� ��$
��Left=�$��3,3,0,0q�$	��Top=�$��28��$��78��$��2��$��2���� -6   $ systemLabel��$�� ��$��Center=�$��	4,5,116,6q�$��Center=�$	��150��$��29��$��2���� $��SQL Installation��$
��Left=�$��
10,229,0,0q�$	��Top=�$	��154��$	��821��$��2��$��2��.���  �� $
��340* ��� $	��61* �.+��� -7   $ sqlCmdTextbox��$
��Left=�$	��112��$��	10,10,0,0q�$
	 Wrap=�$	��Top=�$	��668���� -8   $��Install��$
��Left=�$��3.118,10,0,0q�$	��Top=�$	��109��$	��112��$��1���� $��DB Operations��$
��Left=�$��	12,10,0,0q�$	��Top=�$	��214��$	��431��$��1��.��� $��2,0,-2,0q�.+��� -9   $ sqlInstanceCombobox��$
��Left=�$��	39,31,0,0q�$	��Top=�$	��142��$ 	0.812,0.5&��� -:   $ sqlDBCombobox��$
��Left=�$��
238,31,0,0q�$	��Top=�$	��132���� $��Select SQL instance:��$
��Left=�$��51,5,0,0q�$	��Top=��� $��Select Database:��$
��Left=�$��	258,5,0,0q�$	��Top=�$ 0.478,0.038&��� $��
Backup to:��$
��Left=�$��	10,53,0,0q�$	��Top=�$ 0.534,0&��� -;   $ backupPathTextbox��$
��Left=�$��34��$��	10,79,0,0q�$
	 Wrap=�$	��Top=�$	��318��$ 0.501,-0.261&�$��Center=�$��Center=��� -<   $ backupdbButton��$��Backup��$
��Left=�$��
333,79,0,0q�$	��Top=�$��81��$ 	1.095,0.5&�$��34���� $��Restore from:��$
��Left=�$��
10,113,0,0q�$	��Top=��� -=   $ restorePathTextbox��$
��Left=�$��43��$��
10,139,0,0q�$
	 Wrap=�$	��Top=�$	��156��$ 0.501,-0.261&�$��Center=�$��Center=��� ->   $ restoredbButton��$��Restore��$
��Left=�$��333,139,0,0q�$	��Top=�$��81��$ 	1.095,0.5&�$��43���� $	��As:��$
��Left=�$��238,118,0,0q�$	��Top=�$ 1.802,0.577&��� -?   $ restoredDBnameTextbox��$
��Left=�$��43��$��239,139,0,0q�$
	 Wrap=�$	��Top=�$��89��$ 0.501,-0.261&�$��Center=�$��Center=�$��SmartTradeV6���� -@   $ selectfileButton��$��Select file��$
��Left=�$��171,138,0,0q�$	��Top=�$��62��$ 	1.095,0.5&�$��44���� $��Other��$
��Left=�$��20��$	��Top=�$��77��$��-1,0,0,0q�$ 0.502,0.167&�.��� ���	#FFE5E5E5 ��  �� .+��� -A   $ firewallButton��$��Open port in firewall��$
��Left=�$��
10,151,0,0q�$	��Top=�$	��816��$��42���� -B   $ porttextBox��$
��Left=�$��42��$��
10,198,0,0q�$
	 Wrap=�$
��4000��$	��Top=�$	��816��$��Center=�$��Center=��� -C   $ permissionsButton��$��Set permissions��$
��Left=�$��	10,10,0,0q�$	��Top=�$	��816��$��42���� -D   $ shareButton��$��Share folder��$
��Left=�$��	10,57,0,0q�$	��Top=�$	��816��$��42��$ 0.521,0.224&��� -E   $ backup_button��$)��#Create scheduled task for DB backup��$
��Left=�$��
10,104,0,0q�$	��Top=�$	��816��$��42��$ 0.521,0.224&�
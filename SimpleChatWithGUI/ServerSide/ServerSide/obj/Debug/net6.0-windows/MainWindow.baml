   M S B A M L   `   `   `  ����E  AServerSide, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:ServerSide
ServerSide  5   	       ServerSide.MainWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:ServerSide     5   	   D @ModernWpf, Version=0.9.6.0, Culture=neutral, PublicKeyToken=nullM IModernWpf.Controls, Version=0.9.6.0, Culture=neutral, PublicKeyToken=null,ui!http://schemas.modernwpf.com/2019   5   	   0  *ModernWpf.Controls.Primitives.WindowHelper    UseModernWindowStyle$
  True��5   	    � 
ResizeMode$ NoResize=�5
   	    � Title$ 
MainWindow��5   	   $	��459��6   $	��800��6)   .��5      � .+�5   
   �� -   5         ��Name$ 
logTextBox��5      " �� VerticalScrollBarVisibility$
 Auto=�6&    �� 
IsReadOnly$
 True��6I   $��
43,31,43,0q�6[    �� TextWrapping$
 Wrap=�6o   $��LogInformationHere��6�   $	��Top=�6�   $	��315��6�   �� 5   
   -   5        $ 	ipTextBox��5      $
��Left=�6%   $��
43,382,0,0q�6@   $
 Wrap=�6T   $��	127.0.0.1��6h   $	��Top=�6y   $	��156��6�   �� 5   
   -   5        $ portTextBox��5      $
��Left=�6'   $��236,382,0,0q�6B   $
 Wrap=�6W   $��11000��6k   $	��Top=�6x   $	��120��6�   �� 5   
   $��LOG INFORMATION:��6   $
��Left=�6+   $��	43,12,0,0q�6F   $	��Top=�6Y   �� 5   
   $��IP ADDRESS: ��6   $
��Left=�6'   $��
43,363,0,0q�6B   $	��Top=�6V   �� 5   
   $��PORT: ��6   $
��Left=�6!   $��236,363,0,0q�6<   $	��Top=�6Q   �� 5   
   -   5         ��Name$ startServer��5      $��START SERVER��6'   $��636,382,0,0q�6>   $	��Top=�6S   $	��121��6k   6w   5      5      
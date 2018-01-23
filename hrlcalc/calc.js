exports.calculate = function (co){
var eeisus=0;var eetrue="TRUE";var eefalse="FALSE";var eedec=",";var eeth=".";var eedecreg=new RegExp(",","g");var eethreg=new RegExp("\\.","g");var eecurrencyreg=new RegExp("RSD","g");var eepercentreg=new RegExp("%","g"); var fmtdaynamesshort=new Array("ned","pon","uto","sre","čet","pet","sub"); var fmtdaynameslong=new Array("nedelja","ponedeljak","utorak","sreda","četvrtak","petak","subota"); var fmtmonthnamesshort=new Array("jan","feb","mar","apr","maj","jun","jul","avg","sep","okt","nov","dec"); var fmtmonthnameslong=new Array("januar","februar","mart","april","maj","jun","jul","avgust","septembar","oktobar","novembar","decembar"); var fmtstrings=new Array("."); var fmtdate1=[7,32,3,32,11,32];
var fnCalls= new Array();
var EE= undefined
function calc(data){    
var eecm1=new Array();
var c2E79=data['xlew_2_79_5'];var c2E78=data['xlew_2_78_5'];var c2E77=data['xlew_2_77_5'];var c2E74=data['xlew_2_74_5'];var c2E73=data['xlew_2_73_5'];var c2E72=data['xlew_2_72_5'];var c2E71=data['xlew_2_71_5'];var c2E70=data['xlew_2_70_5'];var c2E69=data['xlew_2_69_5'];var c2E68=data['xlew_2_68_5'];var c2E67=data['xlew_2_67_5'];var c2E66=data['xlew_2_66_5'];var c2E65=data['xlew_2_65_5'];var c2E64=data['xlew_2_64_5'];var c2E63=data['xlew_2_63_5'];var c2E62=data['xlew_2_62_5'];var c2E61=data['xlew_2_61_5'];var c2E60=data['xlew_2_60_5'];var c2E59=data['xlew_2_59_5'];var c2E58=data['xlew_2_58_5'];var c2E57=data['xlew_2_57_5'];var c2E56=data['xlew_2_56_5'];var c2E55=data['xlew_2_55_5'];var c2B55=data['uid'];var c2E54=data['xlew_2_54_5'];var c2E53=data['xlew_2_53_5'];var c2E52=data['xlew_2_52_5'];var c2E51=data['xlew_2_51_5'];var c2E50=data['xlew_2_50_5'];var c2E49=data['xlew_2_49_5'];var c2E48=data['xlew_2_48_5'];var c2E47=data['xlew_2_47_5'];var c2E46=data['xlew_2_46_5'];var c2E45=data['xlew_2_45_5'];var c2E44=data['xlew_2_44_5'];var c2B44=data['inp_new'];var c2E43=data['xlew_2_43_5'];var c2E42=data['xlew_2_42_5'];var c2E41=data['xlew_2_41_5'];var c2E40=data['xlew_2_40_5'];var c2B40=data['tip_fin'];var c2E39=data['xlew_2_39_5'];var c2E38=data['xlew_2_38_5'];var c2E37=data['xlew_2_37_5'];var c2E36=data['xlew_2_36_5'];var c2C36=data['inp_currency'];
var c2B36=data['Inp_finance_amount'];var c2E35=data['xlew_2_35_5'];var c2E34=data['xlew_2_34_5'];var c2E33=data['xlew_2_33_5'];var c2E32=data['xlew_2_32_5'];var c2B32=data['inp_leasing_subject'];var c2D24=data['CURRENT_USER_EMAIL_ADDRESS_HASH'];var c1A34=(today());var c2E6=(0.01914);var c2J6=(0.01716);var c2E7=(0.01914);var c2J7=(0.0165);var c2E8=(0.01914);var c2J8=(0.01595);var c2I15=("170a4b66e0b44070c12b40e84a8dbf959adfef6d125e2ecdd667e0263ca30532638daa783a7f8cd6467d2ecb62b4dcb377f27ac7a3b13d64a6eb86aa2a664b96");var c1A5=(((str_eq((c2B32),("")))?(""):(c2B32)));var c1A9=(((var_eq((c2B36),("")))?(""):(((c2B36=="")?0:c2B36))));var c1B9=(((str_eq((c2C36),("")))?(""):(c2C36)));var c1A13=(((str_eq((c2B40),("")))?(""):(c2B40)));var c1A17=(((str_eq((c2B44),("")))?(""):(c2B44)));var tmp1=(str_eq((c2B40),("FL")));var tmp2=(str_eq((c2B44),("YES")));var tmp3=(var_ls((c2B36),(100000)));var sumcnt4_sum=(((true&&tmp3)&&tmp2)&&tmp1);var sumcnt4_cnt=3;var tmp5=(str_eq((c2B40),("OF")));var tmp6=(str_eq((c2B40),("OL")));var sumcnt7_sum=((false||tmp6)||tmp5);var sumcnt7_cnt=2;var tmp8=(orgeneral(0,sumcnt7_sum,sumcnt7_cnt,eecm1));var tmp9=(str_eq((c2B44),("YES")));var tmp10=(var_ls((c2B36),(100000)));var sumcnt11_sum=(((true&&tmp10)&&tmp9)&&tmp8);var sumcnt11_cnt=3;var tmp12=(str_eq((c2B40),("FL")));var tmp13=(str_eq((c2B44),("YES")));var tmp14=(var_ge((c2B36),(100000)));var tmp15=(var_ls((c2B36),(200000)));var sumcnt16_sum=((((true&&tmp15)&&tmp14)&&tmp13)&&tmp12);var sumcnt16_cnt=4;var tmp17=(str_eq((c2B40),("OF")));var tmp18=(str_eq((c2B40),("OL")));var sumcnt19_sum=((false||tmp18)||tmp17);var sumcnt19_cnt=2;var tmp20=(orgeneral(0,sumcnt19_sum,sumcnt19_cnt,eecm1));var tmp21=(str_eq((c2B44),("YES")));var tmp22=(var_ge((c2B36),(100000)));var tmp23=(var_ls((c2B36),(200000)));var sumcnt24_sum=((((true&&tmp23)&&tmp22)&&tmp21)&&tmp20);var sumcnt24_cnt=4;var tmp25=(str_eq((c2B40),("FL")));var tmp26=(str_eq((c2B44),("YES")));var tmp27=(var_ge((c2B36),(200000)));var sumcnt28_sum=(((true&&tmp27)&&tmp26)&&tmp25);var sumcnt28_cnt=3;var tmp29=(str_eq((c2B40),("OF")));var tmp30=(str_eq((c2B40),("OL")));var sumcnt31_sum=((false||tmp30)||tmp29);var sumcnt31_cnt=2;var tmp32=(orgeneral(0,sumcnt31_sum,sumcnt31_cnt,eecm1));var tmp33=(str_eq((c2B44),("YES")));var tmp34=(var_ge((c2B36),(200000)));var sumcnt35_sum=(((true&&tmp34)&&tmp33)&&tmp32);var sumcnt35_cnt=3;var c1B22=(((andgeneral(0,sumcnt4_sum,sumcnt4_cnt,eecm1))?(((v2n(c2B36))*(c2E6))):((andgeneral(0,sumcnt11_sum,sumcnt11_cnt,eecm1))?(((v2n(c2B36))*(c2E6))):(((andgeneral(0,sumcnt16_sum,sumcnt16_cnt,eecm1))?(((v2n(c2B36))*(c2E7))):((andgeneral(0,sumcnt24_sum,sumcnt24_cnt,eecm1))?(((v2n(c2B36))*(c2E7))):(((andgeneral(0,sumcnt28_sum,sumcnt28_cnt,eecm1))?(((v2n(c2B36))*(c2E8))):((andgeneral(0,sumcnt35_sum,sumcnt35_cnt,eecm1))?(((v2n(c2B36))*(c2E8))):(0))))))))));
 var tmp36=(str_eq((c2B40),("FL")));var tmp37=(str_eq((c2B44),("YES")));var tmp38=(var_ls((c2B36),(100000)));var sumcnt39_sum=(((true&&tmp38)&&tmp37)&&tmp36);var sumcnt39_cnt=3;var tmp40=(str_eq((c2B40),("OL")));var tmp41=(str_eq((c2B40),("OF")));var sumcnt42_sum=((false||tmp41)||tmp40);var sumcnt42_cnt=2;var tmp43=(orgeneral(0,sumcnt42_sum,sumcnt42_cnt,eecm1));var tmp44=(str_eq((c2B44),("YES")));var tmp45=(var_ls((c2B36),(100000)));var sumcnt46_sum=(((true&&tmp45)&&tmp44)&&tmp43);var sumcnt46_cnt=3;var tmp47=(str_eq((c2B40),("FL")));var tmp48=(str_eq((c2B44),("YES")));var tmp49=(var_ge((c2B36),(100000)));var tmp50=(var_ls((c2B36),(200000)));var sumcnt51_sum=((((true&&tmp50)&&tmp49)&&tmp48)&&tmp47);var sumcnt51_cnt=4;var tmp52=(str_eq((c2B40),("OF")));var tmp53=(str_eq((c2B40),("OL")));var sumcnt54_sum=((false||tmp53)||tmp52);var sumcnt54_cnt=2;var tmp55=(orgeneral(0,sumcnt54_sum,sumcnt54_cnt,eecm1));var tmp56=(str_eq((c2B44),("YES")));var tmp57=(var_ge((c2B36),(100000)));var tmp58=(var_ls((c2B36),(200000)));var sumcnt59_sum=((((true&&tmp58)&&tmp57)&&tmp56)&&tmp55);var sumcnt59_cnt=4;var tmp60=(str_eq((c2B40),("FL")));var tmp61=(str_eq((c2B44),("YES")));var tmp62=(var_ge((c2B36),(200000)));var tmp63=(var_le((c2B36),(300000)));var sumcnt64_sum=((((true&&tmp63)&&tmp62)&&tmp61)&&tmp60);var sumcnt64_cnt=4;var tmp65=(str_eq((c2B40),("OF")));var tmp66=(str_eq((c2B40),("OL")));var sumcnt67_sum=((false||tmp66)||tmp65);var sumcnt67_cnt=2;var tmp68=(orgeneral(0,sumcnt67_sum,sumcnt67_cnt,eecm1));var tmp69=(str_eq((c2B44),("YES")));var tmp70=(var_ge((c2B36),(200000)));var tmp71=(var_le((c2B36),(300000)));var sumcnt72_sum=((((true&&tmp71)&&tmp70)&&tmp69)&&tmp68);var sumcnt72_cnt=4;var c1B23=(((andgeneral(0,sumcnt39_sum,sumcnt39_cnt,eecm1))?(((v2n(c2B36))*(c2J6))):((andgeneral(0,sumcnt46_sum,sumcnt46_cnt,eecm1))?(((v2n(c2B36))*(c2J6))):(((andgeneral(0,sumcnt51_sum,sumcnt51_cnt,eecm1))?(((v2n(c2B36))*(c2J7))):((andgeneral(0,sumcnt59_sum,sumcnt59_cnt,eecm1))?(((v2n(c2B36))*(c2J7))):(((andgeneral(0,sumcnt64_sum,sumcnt64_cnt,eecm1))?(((v2n(c2B36))*(c2J8))):((andgeneral(0,sumcnt72_sum,sumcnt72_cnt,eecm1))?(((v2n(c2B36))*(c2J8))):(0))))))))));
 var c1A28=(((str_eq((c2B55),("")))?(""):(c2B55)));var tmp73=(var_eq((c2D24),("")));var tmp74=(var_eq((c2D24),(0)));var sumcnt75_sum=((false||tmp74)||tmp73);var sumcnt75_cnt=2;var tmp76=(var_eq((c2D24),(c2I15)));var sumcnt77_sum=(false||tmp76);var sumcnt77_cnt=1;var c2E26=(((orgeneral(0,sumcnt75_sum,sumcnt75_cnt,eecm1))?(1):((orgeneral(0,sumcnt77_sum,sumcnt77_cnt,eecm1))?(0):(1))));var c6A1=(ssccf1n(("showhide"),(""),("_css8b3d"),(c2E26)));var c6B1=(ssccf1n(("showhide"),(""),("_csscdf6"),(c2E26)));data['XLEW_2_26_5']=c2E26;data['date']=c1A34;data['out_uid']=c1A28;data['out_wiener']=c1B23;data['out_uniqa']=c1B22;data['XLEW_1_17_1']=c1A17;data['XLEW_1_13_1']=c1A13;data['XLEW_1_9_2']=c1B9;data['XLEW_1_9_1']=c1A9;data['XLEW_1_5_1']=c1A5; data['fnCalls']= fnCalls; if(typeof EE !== 'undefined') { data['EE'] = EE; } return JSON.stringify(data);};
function ssccf1n(op,id,css,data1) {
                                        var op = JSON.stringify(op);		
                                        var id = JSON.stringify(id);
                                        var css = JSON.stringify(css);
                                        var data1 = JSON.stringify(data1);
	                                    fnCalls.push('ssccf1n('+ op + ',' + id + ',' + css + ',' + data1 + ')');	
                                    }
                                    function ssccf4n(op,id,colors,data1,data2,data3,data4){
										var op = JSON.stringify(op);		
                                        var id = JSON.stringify(id);
										var colors = JSON.stringify(colors);                                
                                        var data1 = JSON.stringify(data1);
										var data2 = JSON.stringify(data2);
										var data3 = JSON.stringify(data3);
										var data4 = JSON.stringify(data4);
	                                    fnCalls.push('ssccf4n('+ op + ',' + id + ',' + colors + ',' + data1 + ',' + data2 + ',' + data3 + ',' + data4 + ')');
									}
function str_eq(x,y){if(isNaN(x)&&isNaN(y))return(x.toLowerCase()==y.toLowerCase());else return x==y;};

function str_ge(x,y){if(isNaN(x)&&isNaN(y))return(x.toLowerCase()>=y.toLowerCase());else return x>=y;};

function str_ls(x,y){if(isNaN(x)&&isNaN(y))return(x.toLowerCase()<y.toLowerCase());else return x<y;};

function str_le(x,y){if(isNaN(x)&&isNaN(y))return(x.toLowerCase()<=y.toLowerCase());else return x<=y;};

function var_eq(x,y){var xt=mytypeof(x);var yt=mytypeof(y);if(xt!=yt)return false;switch(xt){case 1:case 3:return(x==y);case 2:return str_eq(x,y);default:return false;}};

function var_ge(x,y){var xt=mytypeof(x);var yt=mytypeof(y);if(xt!=yt)return(xt>=yt);switch(xt){case 1:case 3:return(x>=y);case 2:return str_ge(x,y);default:return false;}};

function var_ls(x,y){var xt=mytypeof(x);var yt=mytypeof(y);if(xt!=yt)return(xt<yt);switch(xt){case 1:case 3:return(x<y);case 2:return str_ls(x,y);default:return false;}};

function var_le(x,y){var xt=mytypeof(x);var yt=mytypeof(y);if(xt!=yt)return(xt<=yt);switch(xt){case 1:case 3:return(x<=y);case 2:return str_le(x,y);default:return false;}};























function eegetdropdownvalue(ctl){return ctl.selectedIndex>=0&&ctl[ctl.selectedIndex]?ctl[ctl.selectedIndex].value:0}

function mytypeof(v){switch(typeof v){case "number":if(myIsNaN(v))return 4;return 1;case "string":return 2;case "boolean":return 3;case "object":if(v.constructor==Number){if(myIsNaN(v))return 4;return 1;};if(v.constructor==String){return 2;};if(v.constructor==Boolean){return 3;};return 4;default:return 4;}};

function myIsNaN(x){return(isNaN(x)||(typeof x=='number'&&!isFinite(x)));};

function mod(n,d){return n-d*Math.floor(n/d);};

function round(n,nd){if(isFinite(n)&&isFinite(nd)){var sign_n=(n<0)?-1:1;var abs_n=Math.abs(n);var factor=Math.pow(10,nd);return sign_n*Math.round(abs_n*factor)/factor;}else{return NaN;}};

function andgeneral(cnt,vsum,vcnt,x){if(!vsum){return false;};for(var ii=0;ii<x.length;ii++){var arr=x[ii][0];for(var jj=x[ii][1];jj<=x[ii][3];jj++){for(var kk=x[ii][2];kk<=x[ii][4];kk++){if(!arr[jj][kk]){return false;};};};};return true;};

function orgeneral(cnt,vsum,vcnt,x){if(vsum){return true;};for(var ii=0;ii<x.length;ii++){var arr=x[ii][0];for(var jj=x[ii][1];jj<=x[ii][3];jj++){for(var kk=x[ii][2];kk<=x[ii][4];kk++){if(arr[jj][kk]){return true;};};};};return false;};

function s2n(str){str=String(str).replace(eedecreg,".");str=str.replace(eethreg,"");str=str.replace(eecurrencyreg,"");var res=parseFloat(str);if(myIsNaN(res))res=0;if(str.search(eepercentreg)>=0)res=res/100;return res;}

function b2s(b){return b?eetrue:eefalse;};

function v2n(v){switch(typeof v){case "number":return v;case "string":return s2n(v);case "boolean":return v?1:0;case "object":if(v.constructor==Number){return v;};if(v.constructor==String){return s2n(v);};if(v.constructor==Boolean){return v?1:0;};return Number.NaN;default:return Number.NaN;}};

var near0RegExp=new RegExp("[.](.*0000000|.*9999999)");function eedisplayFloat(x){if(myIsNaN(x)){return Number.NaN;}else{var str=String(x);if(near0RegExp.test(str)){x=round(x,8);str=String(x);}return str.replace(/\./g,eedec);}};

function eedisplayFloatV(x){if(x=="")return x;if(isFinite(x)){return eedisplayFloat(x);}else{return x}};

function eedisplayScientific(x,nd){if(myIsNaN(x)){return Number.NaN;}else{var str=String(x.toExponential(nd));return str.replace(/\./g,eedec);}};

function eedisplayFloatND(x,nd){if(myIsNaN(x)){return Number.NaN;}else{var res=round(x,nd);if(nd>0){var str=String(res);if(str.indexOf('e')!=-1)return str;if(str.indexOf('E')!=-1)return str;var parts=str.split('.');if(parts.length<2){var decimals=('00000000000000').substring(0,nd);return(parts[0]).toString()+eedec+decimals;}else{var decimals=((parts[1]).toString()+'00000000000000').substring(0,nd);return(parts[0]).toString()+eedec+decimals;}}else{return res;}}};

function eedisplayPercent(x){if(myIsNaN(x)){return Number.NaN;}else{return eedisplayFloat(x*100)+'%';}};

function eedisplayPercentND(x,nd){if(myIsNaN(x)){return Number.NaN;}else{return eedisplayFloatND(x*100,nd)+'%';}}

function eedisplayFloatNDTh(x,nd){if(myIsNaN(x)){return Number.NaN;}else{var res=round(x,nd);if(nd>0){var str=String(res);if(str.indexOf('e')!=-1)return str;if(str.indexOf('E')!=-1)return str;var parts=str.split('.');var res2=eeinsertThousand(parts[0].toString());if(parts.length<2){var decimals=('00000000000000').substring(0,nd);return(res2+eedec+decimals);}else{var decimals=((parts[1]).toString()+'00000000000000').substring(0,nd);return(res2+eedec+decimals);}}else{return(eeinsertThousand(res.toString()));}}};

function eedisplayPercentNDTh(x,nd){if(myIsNaN(x)){return Number.NaN;}else{return eedisplayFloatNDTh(x*100,nd)+'%';}}

var eeparseFloatVreg=new RegExp("^ *-?[0-9.]+ *$");function eeparseFloatV(str){if(str=="")return str;str=String(str).replace(eedecreg,".");if(!eeparseFloatVreg.test(str)){return str;};var res=parseFloat(str);if(isNaN(res)){return str;}else{return res;}};

function eedisplayFloatNDThV(x,nd){if(x=="")return x;if(isFinite(x)){return eedisplayFloatNDTh(x,nd)}else{return x}};

var eeparseFloatThVreg=new RegExp("^ *-?[0-9.]+ *$");function eeparseFloatThV(str){if(str=="")return str;str=String(str).replace(eethreg,"");str=String(str).replace(eedecreg,".");if(!eeparseFloatThVreg.test(str)){return str;};var res=parseFloat(str);if(isNaN(res)){return str;}else{return res;}};

function eeinsertThousand(whole){if(whole==""||whole.indexOf("e")>=0){return whole;}else{var minus_sign="";if(whole.charAt(0)=="-"){minus_sign="-";whole=whole.substring(1);};var res="";var str_length=whole.length-1;for(var ii=0;ii<=str_length;ii++){if(ii>0&&ii%3==0){res=eeth+res;};res=whole.charAt(str_length-ii)+res;};return minus_sign+res;}};

 function eedatefmt(fmt,x){if(!isFinite(x))return Number.NaN;var padding=0;var tmp=0;var res="";var len=fmt.length;for(var ii=0;ii<len;ii++){if(fmt[ii]>31){res+=fmtstrings[fmt[ii]-32];}else{switch(fmt[ii]){case 2:res+=eemonth(x);break;case 3:tmp=eemonth(x);if(tmp<10){res+="0";};res+=tmp;break;case 4:res+=fmtmonthnamesshort[eemonth(x)-1];break;case 5:res+=fmtmonthnameslong[eemonth(x)-1];break;case 6:res+=eeday(x);break;case 7:tmp=eeday(x);if(tmp<10){res+="0";};res+=tmp;break;case 8:res+=fmtdaynamesshort[weekday(x,1)-1];break;case 9:res+=fmtdaynameslong[weekday(x,1)-1];break;case 10:tmp=year(x)%100;if(tmp<10){res+="0";};res+=tmp;break;case 11:res+=year(x);break;case 12:res+=hour(x);break;case 13:tmp=hour(x);if(tmp<10){res+="0";};res+=tmp;break;case 14:tmp=hour(x)%12;if(tmp==0){res+="12";}else{res+=tmp%12;};break;case 15:tmp=hour(x)%12;if(tmp==0){res+="12";}else{if(tmp<10){res+="0";};res+=tmp;};break;case 16:res+=minute(x);break;case 17:tmp=minute(x);if(tmp<10){res+="0";};res+=tmp;break;case 18:res+=second(x);break;case 19:tmp=second(x);
 if(tmp<10){res+="0";};res+=tmp;break;case 21:case 22:if(hour(x)<12){res+="AM";}else{res+="PM";};break;case 23:res+=eedisplayFloat(x);break;case 24:tmp=fmt[++ii];res+=eedisplayFloatND(x,tmp);break;case 25:tmp=fmt[++ii];res+=eedisplayFloatNDTh(x,tmp);break;case 26:res+=eedisplayPercent(x);break;case 27:tmp=fmt[++ii];res+=eedisplayPercentND(x,tmp);break;case 28:tmp=fmt[++ii];res+=eedisplayPercentNDTh(x,tmp);break;case 29:tmp=fmt[++ii];res+=eedisplayScientific(x,tmp);break;case 30:padding=fmt[++ii];tmp=hour(x)+Math.floor(x)*24;tmp=tmp.toString();if(tmp.length<padding){res+=('00000000000000').substring(0,padding-tmp.length);}res+=tmp;break;};};};return res;};

function eeisstring(v){switch(typeof v){case "string":return true;case "object":return v.constructor==String;default:return false;}};

function eeunpackdropdownvalue(val) {if (val.length<2) return val;var typ = val.substring(0, 2);var data = val.substring(2);if (typ == 'b:') return data.toLowerCase() == 'true';if (typ == 'n:') return parseFloat(data);if (typ == 's:') return data;return val;  };

function eeisnumber(v){if((v==null)||isNaN(v)||v==Number.NEGATIVE_INFINITY||v==Number.POSITIVE_INFINITY){return false;}else{switch(typeof v){case "number":return true;case "object":return v.constructor==Number;default:return false;}}};

 function leap_gregorian(year){return((year%4)==0)&&(!(((year%100)==0)&&((year%400)!=0)));}var GREGORIAN_EPOCH=1721425;function gregorian_to_jd(year,month,day){return(GREGORIAN_EPOCH-0)+(365*(year-1))+Math.floor((year-1)/4)+(-Math.floor((year-1)/100))+Math.floor((year-1)/400)+Math.floor((((367*month)-362)/12)+((month<=2)?0:(leap_gregorian(year)?-1:-2))+day);}function jd_to_gregorian(jd){var wjd,depoch,quadricent,dqc,cent,dcent,quad,dquad,yindex,year,yearday,leapadj;wjd=Math.floor(jd);depoch=wjd-GREGORIAN_EPOCH-1;quadricent=Math.floor(depoch/146097);dqc=mod(depoch,146097);cent=Math.floor(dqc/36524);dcent=mod(dqc,36524);quad=Math.floor(dcent/1461);dquad=mod(dcent,1461);yindex=Math.floor(dquad/365);year=(quadricent*400)+(cent*100)+(quad*4)+yindex;if(!((cent==4)||(yindex==4))){year++;}yearday=wjd-gregorian_to_jd(year,1,1);leapadj=((wjd<gregorian_to_jd(year,3,1))?0:(leap_gregorian(year)?1:2));var month=Math.floor((((yearday+leapadj)*12)+373)/367);var day=(wjd-gregorian_to_jd(year,month,1))+1;
 return new Array(year,month,day);}

 function date(year,month,day){if(!isFinite(day)||!isFinite(month)||!isFinite(year))return Number.NaN;if(year<1900){year+=1900};if(year>9999)return Number.NaN;var adj_year=year;var adj_month=month;if(month>0){adj_year=year+Math.floor((month-1)/12);adj_month=((month-1)%12)+1;}else if(month<0){var tmp=Math.ceil((-month)/12);adj_year=year-tmp;adj_month=adj_month+tmp*12;}var res=Math.floor(gregorian_to_jd(adj_year,adj_month,day)-2415020);if(res>59)return res+1;return res;};

function eeday(serial_number){if(!isFinite(serial_number))return Number.NaN;if(serial_number<1){return 0;}if(serial_number>60)serial_number--;var res=jd_to_gregorian(serial_number+2415020);return res[2];};

function hour(serial_number){if(!isFinite(serial_number))return Number.NaN;var res=Math.floor((serial_number-Math.floor(serial_number))*86400+0.5);return Math.floor(res/3600);}

function minute(serial_number){if(!isFinite(serial_number))return Number.NaN;var res=Math.floor((serial_number-Math.floor(serial_number))*86400+0.5);return Math.floor(res/60)%60;};

function eemonth(serial_number){if(!isFinite(serial_number))return Number.NaN;if(serial_number<1){return 1;}if(serial_number>60)serial_number--;var res=jd_to_gregorian(serial_number+2415020);return res[1];};

function second(serial_number){if(!isFinite(serial_number))return Number.NaN;var res=Math.floor((serial_number-Math.floor(serial_number))*86400+0.5);return res%60;};

function today(){var dte=new Date();return date(dte.getFullYear(),dte.getMonth()+1,dte.getDate());};

 function weekday(serial_number,return_type){if(!isFinite(return_type)||!isFinite(serial_number))return Number.NaN;if(return_type<1||return_type>3)return Number.NaN;var res=Math.floor(serial_number+6)%7;switch(Math.floor(return_type)){case 1:return res+1;case 2:return(res+6)%7+1;case 3:return(res+6)%7;};return "hej";};

function year(serial_number){if(!isFinite(serial_number))return Number.NaN;if(serial_number<1){return 1900;}if(serial_number>60)serial_number--;var res=jd_to_gregorian(serial_number+2415020);return res[0];};
return calc(co);
}

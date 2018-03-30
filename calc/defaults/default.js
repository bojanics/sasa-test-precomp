exports.calculate = function (co){
   function calc(data){   
      if (data==null) {
         data = {};
      }
      var isxok = true;
      var isyok = true;
      var x = data['x'];
      if (typeof x !='number') {
         isxok = false;
      }
      var y = data['y'];
      if (typeof y !='number') {
         isyok = false;
      }
      var z = null;
      var msg = null;
      if (isxok && isyok) {
         z = x+y;    
         msg="The sum of "+x+" and "+y+" is "+z;
      } else {
         msg="Please provide parameters x and y which are numbers. Unable to perform calculation with parameters x="+(!isxok ? "'" : "")+x+(!isxok ? "'" : "")+" and y="+(!isyok ? "'" : "")+y+(!isyok ? "'" : "");
      }      
      data['z']=z; 
      data['message']=msg;
      return JSON.stringify(data);
   };
   return calc(co);
}

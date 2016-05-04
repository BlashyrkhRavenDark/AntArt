using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace WindowsFormsApplication1
{
    class AACard
    {
        private String m_sName;
        private Image m_imgBig;
        private Image m_imgSmall;
        private int m_iId;

        public AACard(int p_iId)
        {

        }

        public AACard(string p_sName)
        {

        }

        public AACard(string p_sName, Image p_imgBig, Image p_imgSmall, int p_iId)
        {
            m_sName = p_sName;
            m_imgBig = p_imgBig;
            m_imgSmall = p_imgSmall;
            m_iId = p_iId;        
        }

    }
}
